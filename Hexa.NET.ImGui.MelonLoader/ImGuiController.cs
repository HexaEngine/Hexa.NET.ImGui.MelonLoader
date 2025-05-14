namespace Hexa.NET.ImGui.MelonLoader
{
    using HexaGen.Runtime;
    using Il2CppFluffyUnderware.DevTools.Extensions;
    using Il2CppInterop.Runtime;
    using Il2CppInterop.Runtime.InteropTypes.Arrays;
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using UnityEngine;
    using UnityEngine.Rendering;

    public class ImGuiController
    {
        private ImGuiContextPtr context;
        private UniverseLib.AssetBundle bundle = UniverseLib.AssetBundle.LoadFromFile("Mods/ImGui.bundle");
        private Shader shader;
        private Material material;
        private Texture2D fontTexture;

        private long time;
        private Vector2 oldMousePos;
        private Vector2 oldMouseScroll;
        private bool isFocused;
        private static readonly bool[] keyStates = new bool[(int)ImGuiKey.NamedKeyCount];
        private readonly bool[] buttonStates = new bool[5];

        private CommandBuffer buffer = new();
        private Mesh mesh = new();

        private Il2CppStructArray<Vector3> verts;
        private Il2CppStructArray<Vector2> uvs;
        private Il2CppStructArray<Color32> colors;
        private Il2CppStructArray<int> indices;

        private unsafe struct BackendData
        {
            public byte* ClipboardTextData;
        }

        public unsafe ImGuiController(Action<ImGuiIOPtr> configure)
        {
            context = ImGui.CreateContext();
            ImGui.SetCurrentContext(context);
            var io = ImGui.GetIO();
            configure?.Invoke(io);
            io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
            io.BackendPlatformUserData = Utils.Alloc<BackendData>(1);
            io.BackendRendererName = Utils.StringToUTF8Ptr("Unity ImGui");
            io.BackendPlatformName = Utils.StringToUTF8Ptr("Unity ImGui");

            var platformIO = ImGui.GetPlatformIO();

            platformIO.PlatformGetClipboardTextFn = (void*)Marshal.GetFunctionPointerForDelegate<PlatformGetClipboardTextFn>(GetClipboardText);
            platformIO.PlatformSetClipboardTextFn = (void*)Marshal.GetFunctionPointerForDelegate<PlatformSetClipboardTextFn>(SetClipboardText);
            platformIO.PlatformClipboardUserData = null;

            platformIO.PlatformSetImeDataFn = (void*)Marshal.GetFunctionPointerForDelegate<PlatformSetImeDataFn>(SetImeData);
            platformIO.PlatformOpenInShellFn = (void*)Marshal.GetFunctionPointerForDelegate<PlatformOpenInShellFn>(OpenInShell);

            shader = bundle.LoadAsset<Shader>("Assets/ImGuiOverlayShader.shader");
            material = new Material(shader);
            mesh.MarkDynamic();
        }

        public void Reinitialize()
        {
            if (buffer.Pointer != IntPtr.Zero)
                buffer?.Dispose();
            buffer = null;
            if (mesh.Pointer != IntPtr.Zero)
                mesh?.Destroy();
            mesh = null;
            if (material.Pointer != IntPtr.Zero)
                material?.Destroy();
            material = null;

            buffer = new();
            shader = bundle.LoadAsset<Shader>("Assets/ImGuiOverlayShader.shader");
            material = new Material(shader);
            mesh = new();
            mesh.MarkDynamic();
        }

        private unsafe byte OpenInShell(ImGuiContext* ctx, byte* path)
        {
            string url = Utils.DecodeStringUTF8(path);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else
            {
                return 0;
            }

            return 1;
        }

        private static unsafe void SetImeData(ImGuiContext* ctx, ImGuiViewport* viewport, ImGuiPlatformImeData* data)
        {
            bool wantIME = data->WantVisible == 1;
            GUIUtility.imeCompositionMode = wantIME ? IMECompositionMode.On : IMECompositionMode.Auto;
        }

        private static unsafe BackendData* GetBackendData()
        {
            return !ImGui.GetCurrentContext().IsNull ? (BackendData*)ImGui.GetIO().BackendPlatformUserData : null;
        }

        private static unsafe void SetClipboardText(ImGuiContext* ctx, byte* text)
        {
            GUIUtility.systemCopyBuffer = Utils.DecodeStringUTF8(text);
        }

        private static unsafe byte* GetClipboardText(ImGuiContext* ctx)
        {
            BackendData* bd = GetBackendData();
            if (bd->ClipboardTextData != null)
            {
                Utils.Free(bd->ClipboardTextData);
            }
            bd->ClipboardTextData = Utils.StringToUTF8Ptr(GUIUtility.systemCopyBuffer);
            return bd->ClipboardTextData;
        }

        public void NewFrame()
        {
            if (fontTexture == null)
            {
                CreateFontTexture();
            }

            long now = Stopwatch.GetTimestamp();
            float delta;
            if (time == 0)
            {
                delta = 0.0001f;
            }
            else
            {
                delta = (now - time) / (float)Stopwatch.Frequency;
            }
            time = now;

            var io = ImGui.GetIO();
            io.DisplaySize = new(Screen.width, Screen.height);
            io.DeltaTime = delta;

            textureId = 1;
            idToTexture.Clear();
            ImGui.SetCurrentContext(context);
            ImGui.NewFrame();
        }

        public void InvalidateFontTexture()
        {
            if (fontTexture == null) return;
            fontTexture.Destroy();
            fontTexture = null;
        }

        private unsafe void CreateFontTexture()
        {
            var io = ImGui.GetIO();
            byte* data;
            int w, h;
            io.Fonts.GetTexDataAsRGBA32(&data, &w, &h);

            int stride = w * 4;
            int length = stride * h;

            fontTexture = new(w, h, TextureFormat.RGBA32, false, true);
            fontTexture.LoadRawTextureData((IntPtr)data, length);
            fontTexture.Apply();
        }

        private delegate nint get_inputStringDelegate();

        private static readonly get_inputStringDelegate get_inputString = IL2CPP.ResolveICall<get_inputStringDelegate>("UnityEngine.Input::get_inputString");

        public unsafe void UpdateInput()
        {
            var io = ImGui.GetIO();
            Vector2 mousePos = Input.mousePosition;

            if (mousePos != oldMousePos)
            {
                io.AddMouseSourceEvent(ImGuiMouseSource.Mouse);
                io.AddMousePosEvent(mousePos.x, Screen.height - mousePos.y);
                oldMousePos = mousePos;
            }

            HandleMouseButtons(io);

            Vector2 mouseScroll = Input.mouseScrollDelta;
            if (oldMouseScroll != mouseScroll)
            {
                io.AddMouseSourceEvent(ImGuiMouseSource.Mouse);
                io.AddMouseWheelEvent(mouseScroll.x, mouseScroll.y);
                oldMouseScroll = mouseScroll;
            }

            bool isFocusedNow = Application.isFocused;
            if (isFocusedNow != isFocused)
            {
                io.AddFocusEvent(isFocusedNow);
                isFocused = isFocusedNow;
            }

            for (KeyCode i = 0; i < KeyCode.Mouse0; i++)
            {
                var imGuiKey = MapKey(i);
                if (imGuiKey == ImGuiKey.None) continue;
                bool state = Input.GetKey(i);
                int index = imGuiKey - ImGuiKey.NamedKeyBegin;
                if (keyStates[index] == state) continue;
                io.AddKeyEvent(imGuiKey, state);
                keyStates[index] = state;
                var keymod = TranslateKeyMod(imGuiKey);
                if (keymod == ImGuiKey.None) continue;
                io.AddKeyEvent(keymod, state);
            }

            // custom getter for preventing gc pressure and direct translation.
            nint inputString = get_inputString();
            var length = IL2CPP.il2cpp_string_length(inputString);
            var chars = IL2CPP.il2cpp_string_chars(inputString);
            for (int i = 0; i < length; i++)
            {
                io.AddInputCharacterUTF16(chars[i]);
            }
        }

        private static ImGuiKey TranslateKeyMod(ImGuiKey key)
        {
            ImGuiKey keymods = 0;
            if (key == ImGuiKey.LeftCtrl || key == ImGuiKey.RightCtrl)
            {
                keymods = ImGuiKey.ModCtrl;
            }
            if (key == ImGuiKey.LeftShift || key == ImGuiKey.RightShift)
            {
                keymods = ImGuiKey.ModShift;
            }
            if (key == ImGuiKey.LeftAlt || key == ImGuiKey.RightAlt)
            {
                keymods = ImGuiKey.ModAlt;
            }
            if (key == ImGuiKey.LeftSuper || key == ImGuiKey.RightSuper)
            {
                keymods = ImGuiKey.ModSuper;
            }
            return keymods;
        }

        private static ImGuiKey MapKey(KeyCode key)
        {
            return key switch
            {
                KeyCode.None => ImGuiKey.None,
                KeyCode.Backspace => ImGuiKey.Backspace,
                KeyCode.Delete => ImGuiKey.Delete,
                KeyCode.Tab => ImGuiKey.Tab,
                KeyCode.Return => ImGuiKey.Enter,
                KeyCode.Pause => ImGuiKey.Pause,
                KeyCode.Escape => ImGuiKey.Escape,
                KeyCode.Space => ImGuiKey.Space,
                KeyCode.Keypad0 => ImGuiKey.Keypad0,
                KeyCode.Keypad1 => ImGuiKey.Keypad1,
                KeyCode.Keypad2 => ImGuiKey.Keypad2,
                KeyCode.Keypad3 => ImGuiKey.Keypad3,
                KeyCode.Keypad4 => ImGuiKey.Keypad4,
                KeyCode.Keypad5 => ImGuiKey.Keypad5,
                KeyCode.Keypad6 => ImGuiKey.Keypad6,
                KeyCode.Keypad7 => ImGuiKey.Keypad7,
                KeyCode.Keypad8 => ImGuiKey.Keypad8,
                KeyCode.Keypad9 => ImGuiKey.Keypad9,
                KeyCode.KeypadPeriod => ImGuiKey.KeypadDecimal,
                KeyCode.KeypadDivide => ImGuiKey.KeypadDivide,
                KeyCode.KeypadMultiply => ImGuiKey.KeypadMultiply,
                KeyCode.KeypadMinus => ImGuiKey.KeypadSubtract,
                KeyCode.KeypadPlus => ImGuiKey.KeypadAdd,
                KeyCode.KeypadEnter => ImGuiKey.KeypadEnter,
                KeyCode.KeypadEquals => ImGuiKey.KeypadEqual,
                KeyCode.UpArrow => ImGuiKey.UpArrow,
                KeyCode.DownArrow => ImGuiKey.DownArrow,
                KeyCode.RightArrow => ImGuiKey.RightArrow,
                KeyCode.LeftArrow => ImGuiKey.LeftArrow,
                KeyCode.Insert => ImGuiKey.Insert,
                KeyCode.Home => ImGuiKey.Home,
                KeyCode.End => ImGuiKey.End,
                KeyCode.PageUp => ImGuiKey.PageUp,
                KeyCode.PageDown => ImGuiKey.PageDown,
                KeyCode.F1 => ImGuiKey.F1,
                KeyCode.F2 => ImGuiKey.F2,
                KeyCode.F3 => ImGuiKey.F3,
                KeyCode.F4 => ImGuiKey.F4,
                KeyCode.F5 => ImGuiKey.F5,
                KeyCode.F6 => ImGuiKey.F6,
                KeyCode.F7 => ImGuiKey.F7,
                KeyCode.F8 => ImGuiKey.F8,
                KeyCode.F9 => ImGuiKey.F9,
                KeyCode.F10 => ImGuiKey.F10,
                KeyCode.F11 => ImGuiKey.F11,
                KeyCode.F12 => ImGuiKey.F12,
                KeyCode.F13 => ImGuiKey.F13,
                KeyCode.F14 => ImGuiKey.F14,
                KeyCode.F15 => ImGuiKey.F15,
                KeyCode.Alpha0 => ImGuiKey.Key0,
                KeyCode.Alpha1 => ImGuiKey.Key1,
                KeyCode.Alpha2 => ImGuiKey.Key2,
                KeyCode.Alpha3 => ImGuiKey.Key3,
                KeyCode.Alpha4 => ImGuiKey.Key4,
                KeyCode.Alpha5 => ImGuiKey.Key5,
                KeyCode.Alpha6 => ImGuiKey.Key6,
                KeyCode.Alpha7 => ImGuiKey.Key7,
                KeyCode.Alpha8 => ImGuiKey.Key8,
                KeyCode.Alpha9 => ImGuiKey.Key9,
                KeyCode.Quote => ImGuiKey.Apostrophe,
                KeyCode.Comma => ImGuiKey.Comma,
                KeyCode.Minus => ImGuiKey.Minus,
                KeyCode.Period => ImGuiKey.Period,
                KeyCode.Slash => ImGuiKey.Slash,
                KeyCode.Colon => ImGuiKey.Period,
                KeyCode.Semicolon => ImGuiKey.Semicolon,
                KeyCode.LeftBracket => ImGuiKey.LeftBracket,
                KeyCode.Backslash => ImGuiKey.Backslash,
                KeyCode.RightBracket => ImGuiKey.RightBracket,
                KeyCode.A => ImGuiKey.A,
                KeyCode.B => ImGuiKey.B,
                KeyCode.C => ImGuiKey.C,
                KeyCode.D => ImGuiKey.D,
                KeyCode.E => ImGuiKey.E,
                KeyCode.F => ImGuiKey.F,
                KeyCode.G => ImGuiKey.G,
                KeyCode.H => ImGuiKey.H,
                KeyCode.I => ImGuiKey.I,
                KeyCode.J => ImGuiKey.J,
                KeyCode.K => ImGuiKey.K,
                KeyCode.L => ImGuiKey.L,
                KeyCode.M => ImGuiKey.M,
                KeyCode.N => ImGuiKey.N,
                KeyCode.O => ImGuiKey.O,
                KeyCode.P => ImGuiKey.P,
                KeyCode.Q => ImGuiKey.Q,
                KeyCode.R => ImGuiKey.R,
                KeyCode.S => ImGuiKey.S,
                KeyCode.T => ImGuiKey.T,
                KeyCode.U => ImGuiKey.U,
                KeyCode.V => ImGuiKey.V,
                KeyCode.W => ImGuiKey.W,
                KeyCode.X => ImGuiKey.X,
                KeyCode.Y => ImGuiKey.Y,
                KeyCode.Z => ImGuiKey.Z,
                KeyCode.Numlock => ImGuiKey.NumLock,
                KeyCode.CapsLock => ImGuiKey.CapsLock,
                KeyCode.ScrollLock => ImGuiKey.ScrollLock,
                KeyCode.RightShift => ImGuiKey.RightShift,
                KeyCode.LeftShift => ImGuiKey.LeftShift,
                KeyCode.RightControl => ImGuiKey.RightCtrl,
                KeyCode.LeftControl => ImGuiKey.LeftCtrl,
                KeyCode.RightAlt => ImGuiKey.RightAlt,
                KeyCode.LeftAlt => ImGuiKey.LeftAlt,
                KeyCode.LeftCommand => ImGuiKey.LeftSuper,
                KeyCode.LeftWindows => ImGuiKey.LeftSuper,
                KeyCode.RightCommand => ImGuiKey.RightSuper,
                KeyCode.RightWindows => ImGuiKey.RightSuper,
                KeyCode.AltGr => ImGuiKey.RightAlt,
                KeyCode.Print => ImGuiKey.PrintScreen,
                KeyCode.Break => ImGuiKey.Pause,
                KeyCode.Menu => ImGuiKey.Menu,
                _ => ImGuiKey.None,
            };
        }

        private void HandleMouseButtons(ImGuiIOPtr io)
        {
            for (int i = 0; i < 5; i++)
            {
                var state = Input.GetMouseButton(i);
                if (buttonStates[i] != state)
                {
                    io.AddMouseSourceEvent(ImGuiMouseSource.Mouse);
                    io.AddMouseButtonEvent(i, state);
                    buttonStates[i] = state;
                }
            }
        }

        private static Il2CppStructArray<T> Grow<T>(ref Il2CppStructArray<T> array, int size) where T : unmanaged
        {
            if (array == null || array.Length < size)
            {
                array = new(size * 2);
            }
            return array;
        }

        private readonly static Dictionary<ulong, Texture2D> idToTexture = [];
        private static ulong textureId = 1;

        public static ulong RegisterTexture(Texture2D texture)
        {
            var currentId = textureId++;
            idToTexture[currentId] = texture;
            return currentId;
        }

        public unsafe void EndFrame()
        {
            ImGui.Render();
            ImGui.EndFrame();
            Render(ImGui.GetDrawData());
        }

        private unsafe void Render(ImDrawData* data)
        {
            int totalVtxCount = data->TotalVtxCount;
            int totalIdxCount = data->TotalIdxCount;

            if (totalVtxCount == 0 || totalIdxCount == 0)
                return;

            Grow(ref verts, totalVtxCount);
            Grow(ref uvs, totalVtxCount);
            Grow(ref colors, totalVtxCount);
            Grow(ref indices, totalIdxCount);

            int vtxOffset = 0;
            int idxOffset = 0;

            int subMeshCount = 0;
            for (int n = 0; n < data->CmdListsCount; n++)
            {
                ImDrawList* cmdList = data->CmdLists[n];

                for (int i = 0; i < cmdList->VtxBuffer.Size; i++)
                {
                    ImDrawVert* v = cmdList->VtxBuffer.Data + i;
                    verts[vtxOffset + i] = *(Vector2*)&v->Pos;
                    uvs[vtxOffset + i] = *(Vector2*)&v->Uv;
                    colors[vtxOffset + i] = *(Color32*)&v->Col;
                }

                for (int i = 0; i < cmdList->IdxBuffer.Size; i++)
                {
                    indices[idxOffset + i] = cmdList->IdxBuffer[i];
                }

                vtxOffset += cmdList->VtxBuffer.Size;
                idxOffset += cmdList->IdxBuffer.Size;
                subMeshCount += cmdList->CmdBuffer.Size;
            }

            mesh.subMeshCount = subMeshCount;

            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetColors(colors);

            material.SetPass(0);
            material.SetTexture("_FontTexture", fontTexture);

            float L = data->DisplayPos.X;
            float R = data->DisplayPos.X + data->DisplaySize.X;
            float T = data->DisplayPos.Y;
            float B = data->DisplayPos.Y + data->DisplaySize.Y;
            Matrix4x4 mvp = new(
                new Vector4(2.0f / (R - L), 0.0f, 0.0f, 0.0f),
                new Vector4(0.0f, 2.0f / (T - B), 0.0f, 0.0f),
                new Vector4(0.0f, 0.0f, 0.5f, 0.0f),
                new Vector4((R + L) / (L - R), (T + B) / (B - T), 0.5f, 1.0f)
            );

            material.SetMatrix("_ProjectionMatrix", mvp);

            buffer.Clear();

            int global_idx_offset = 0;
            int global_vtx_offset = 0;
            int subMeshIndex = 0;
            for (int n = 0; n < data->CmdListsCount; n++)
            {
                ImDrawList* cmdList = data->CmdLists[n];

                for (int i = 0; i < cmdList->CmdBuffer.Size; i++)
                {
                    var cmd = cmdList->CmdBuffer[i];

                    if (idToTexture.TryGetValue(cmd.TextureId.Handle, out var texture))
                    {
                        material.SetTexture("_FontTexture", texture);
                    }
                    else
                    {
                        material.SetTexture("_FontTexture", fontTexture);
                    }

                    var clip = cmd.ClipRect;
                    Rect scissorRect = new(
                        clip.X,
                        Screen.height - clip.W,
                        clip.Z - clip.X,
                        clip.W - clip.Y
                    );

                    buffer.EnableScissorRect(scissorRect);

                    var srv = cmd.TextureId.Handle;
                    mesh.SetTriangles(indices, global_idx_offset + (int)cmd.IdxOffset, (int)cmd.ElemCount, subMeshIndex, baseVertex: global_vtx_offset + (int)cmd.VtxOffset);
                    buffer.DrawMesh(mesh, Matrix4x4.identity, material, subMeshIndex);
                    subMeshIndex++;

                    buffer.DisableScissorRect();
                }
                global_idx_offset += cmdList->IdxBuffer.Size;
                global_vtx_offset += cmdList->VtxBuffer.Size;
            }

            Graphics.ExecuteCommandBuffer(buffer);
        }

        public unsafe void Dispose()
        {
            var bd = GetBackendData();
            if (bd != null)
            {
                if (bd->ClipboardTextData != null)
                {
                    Utils.Free(bd->ClipboardTextData);
                    bd->ClipboardTextData = null;
                }
                var io = ImGui.GetIO();
                io.BackendPlatformUserData = null;
                Utils.Free(bd);
            }

            InvalidateFontTexture();
            buffer?.Dispose();
            buffer = null;
            mesh?.Destroy();
            mesh = null;
            material?.Destroy();
            material = null;
            shader?.Destroy();
            shader = null;
            bundle?.Destroy();
            bundle = null;
        }
    }
}