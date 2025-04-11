namespace S1Utils.Extensions
{
    using Hexa.NET.Utilities;
    using Hexa.NET.Utilities.Text;
    using Il2CppInterop.Runtime;
    using Il2CppInterop.Runtime.InteropTypes;
    using Il2CppScheduleOne.ItemFramework;

    public static class StrBuilderExtensions
    {
        public static StrBuilder Icon(this ref StrBuilder builder, char c)
        {
            builder.Reset();
            builder.Append(c);
            builder.End();
            return builder;
        }

        public static StrBuilder Icon(this ref StrBuilder builder, char c, ReadOnlySpan<byte> text)
        {
            builder.Reset();
            builder.Append(c);
            builder.Append(text);
            builder.End();
            return builder;
        }

        public static StrBuilder IconId(this ref StrBuilder builder, char c, string id, int mod)
        {
            builder.Reset();
            builder.Append(c);
            builder.Append("##"u8);
            builder.Append(id);
            builder.Append(mod);
            builder.End();
            return builder;
        }

        public static unsafe void Append(this ref StrBuilder builder, char* chars, int length)
        {
            builder.Index += Utf8Formatter.ConvertUtf16ToUtf8(chars, length, builder.Buffer + builder.Index, builder.Count - builder.Index);
        }

        public static unsafe void Append(this ref StrBuilder builder, StdWString cppString)
        {
            builder.Index += Utf8Formatter.ConvertUtf16ToUtf8(cppString.Data, cppString.Size, builder.Buffer + builder.Index, builder.Count - builder.Index);
        }

        private static readonly nint NativeFieldInfoPtr_Name = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<ItemDefinition>.NativeClassPtr, "Name");
        private static readonly nint NativeFieldInfoPtr_ID = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<ItemDefinition>.NativeClassPtr, "ID");

        public static unsafe StdWString GetName(this ItemDefinition definition)
        {
            return Il2CppString(definition, NativeFieldInfoPtr_Name);
        }

        public static unsafe StdWString GetID(this ItemDefinition definition)
        {
            return Il2CppString(definition, NativeFieldInfoPtr_ID);
        }

        private static unsafe StdWString Il2CppString(Il2CppObjectBase definition, nint field)
        {
            nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull(definition) + (int)IL2CPP.il2cpp_field_get_offset(field);
            var il2CppString = *(nint*)num;
            var length = IL2CPP.il2cpp_string_length(il2CppString);
            var chars = IL2CPP.il2cpp_string_chars(il2CppString);

            StdWString str = new(length);
            str.Resize(length);
            Utils.MemcpyT(chars, str.Data, length);

            return str;
        }

        public static unsafe void AppendName(this ItemDefinition definition, ref StrBuilder builder)
        {
            AppendIl2CppString(definition, ref builder, NativeFieldInfoPtr_Name);
        }

        public static unsafe void AppendID(this ItemDefinition definition, ref StrBuilder builder)
        {
            AppendIl2CppString(definition, ref builder, NativeFieldInfoPtr_ID);
        }

        private static unsafe void AppendIl2CppString(Il2CppObjectBase definition, ref StrBuilder builder, nint field)
        {
            nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull(definition) + (int)IL2CPP.il2cpp_field_get_offset(field);
            var length = IL2CPP.il2cpp_string_length(num);
            var chars = IL2CPP.il2cpp_string_chars(num);
            builder.Append(chars, length);
        }
    }

    public unsafe struct IL2CppString
    {
        public char* Data;
        public int Length;

        public IL2CppString(char* data, int length)
        {
            Data = data;
            Length = length;
        }

        public readonly ReadOnlySpan<char> AsSpan()
        {
            return new ReadOnlySpan<char>(Data, Length);
        }
    }
}