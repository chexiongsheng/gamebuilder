using System;
using System.Runtime.CompilerServices;

namespace VYaml.Internal
{
  public static class YamlCodes
  {
    public static readonly byte[] YamlDirectiveName = System.Text.Encoding.UTF8.GetBytes("YAML");
    public static readonly byte[] TagDirectiveName = System.Text.Encoding.UTF8.GetBytes("TAG");

    public static readonly byte[] Utf8Bom = new byte[] { 0xEF, 0xBB, 0xBF };
    public static readonly byte[] StreamStart = System.Text.Encoding.UTF8.GetBytes("---");
    public static readonly byte[] DocStart = System.Text.Encoding.UTF8.GetBytes("...");
    public static readonly byte[] CrLf = System.Text.Encoding.UTF8.GetBytes("\r\n");

    public static readonly byte[] Null0 = System.Text.Encoding.UTF8.GetBytes("null");
    public static readonly byte[] Null1 = System.Text.Encoding.UTF8.GetBytes("Null");
    public static readonly byte[] Null2 = System.Text.Encoding.UTF8.GetBytes("NULL");
    public const byte NullAlias = (byte)'~';

    public static readonly byte[] True0 = System.Text.Encoding.UTF8.GetBytes("true");
    public static readonly byte[] True1 = System.Text.Encoding.UTF8.GetBytes("True");
    public static readonly byte[] True2 = System.Text.Encoding.UTF8.GetBytes("TRUE");

    public static readonly byte[] False0 = System.Text.Encoding.UTF8.GetBytes("false");
    public static readonly byte[] False1 = System.Text.Encoding.UTF8.GetBytes("False");
    public static readonly byte[] False2 = System.Text.Encoding.UTF8.GetBytes("FALSE");

    public static readonly byte[] Inf0 = System.Text.Encoding.UTF8.GetBytes(".inf");
    public static readonly byte[] Inf1 = System.Text.Encoding.UTF8.GetBytes(".Inf");
    public static readonly byte[] Inf2 = System.Text.Encoding.UTF8.GetBytes(".INF");
    public static readonly byte[] Inf3 = System.Text.Encoding.UTF8.GetBytes("+.inf");
    public static readonly byte[] Inf4 = System.Text.Encoding.UTF8.GetBytes("+.Inf");
    public static readonly byte[] Inf5 = System.Text.Encoding.UTF8.GetBytes("+.INF");

    public static readonly byte[] Yes0 = System.Text.Encoding.UTF8.GetBytes("yes");
    public static readonly byte[] Yes1 = System.Text.Encoding.UTF8.GetBytes("Yes");
    public static readonly byte[] Yes2 = System.Text.Encoding.UTF8.GetBytes("YES");

    public static readonly byte[] No0 = System.Text.Encoding.UTF8.GetBytes("no");
    public static readonly byte[] No1 = System.Text.Encoding.UTF8.GetBytes("No");
    public static readonly byte[] No2 = System.Text.Encoding.UTF8.GetBytes("NO");

    public static readonly byte[] On0 = System.Text.Encoding.UTF8.GetBytes("on");
    public static readonly byte[] On1 = System.Text.Encoding.UTF8.GetBytes("On");
    public static readonly byte[] On2 = System.Text.Encoding.UTF8.GetBytes("ON");

    public static readonly byte[] Off0 = System.Text.Encoding.UTF8.GetBytes("off");
    public static readonly byte[] Off1 = System.Text.Encoding.UTF8.GetBytes("Off");
    public static readonly byte[] Off2 = System.Text.Encoding.UTF8.GetBytes("OFF");

    public static readonly byte[] NegInf0 = System.Text.Encoding.UTF8.GetBytes("-.inf");
    public static readonly byte[] NegInf1 = System.Text.Encoding.UTF8.GetBytes("-.Inf");
    public static readonly byte[] NegInf2 = System.Text.Encoding.UTF8.GetBytes("-.INF");

    public static readonly byte[] Nan0 = System.Text.Encoding.UTF8.GetBytes(".nan");
    public static readonly byte[] Nan1 = System.Text.Encoding.UTF8.GetBytes(".NaN");
    public static readonly byte[] Nan2 = System.Text.Encoding.UTF8.GetBytes(".NAN");

    public static readonly byte[] HexPrefix = System.Text.Encoding.UTF8.GetBytes("0x");
    public static readonly byte[] HexPrefixNegative = System.Text.Encoding.UTF8.GetBytes("-0x");

    public static readonly byte[] OctalPrefix = System.Text.Encoding.UTF8.GetBytes("0o");
    public static readonly byte[] UnityStrippedSymbol = System.Text.Encoding.UTF8.GetBytes("stripped");

    public const byte Space = (byte)' ';
    public const byte Tab = (byte)'\t';
    public const byte Lf = (byte)'\n';
    public const byte Cr = (byte)'\r';
    public const byte Comment = (byte)'#';
    public const byte DirectiveLine = (byte)'%';
    public const byte Alias = (byte)'*';
    public const byte Anchor = (byte)'&';
    public const byte Tag = (byte)'!';
    public const byte SingleQuote = (byte)'\'';
    public const byte DoubleQuote = (byte)'"';
    public const byte LiteralScalerHeader = (byte)'|';
    public const byte FoldedScalerHeader = (byte)'>';
    public const byte Comma = (byte)',';
    public const byte BlockEntryIndent = (byte)'-';
    public const byte ExplicitKeyIndent = (byte)'?';
    public const byte MapValueIndent = (byte)':';
    public const byte FlowMapStart = (byte)'{';
    public const byte FlowMapEnd = (byte)'}';
    public const byte FlowSequenceStart = (byte)'[';
    public const byte FlowSequenceEnd = (byte)']';

    static readonly bool[] EmptyTable = new bool[256];
    static readonly bool[] BlankTable = new bool[256];
    static readonly bool[] FlowSymbolTable = new bool[256];

    static YamlCodes()
    {
      EmptyTable[Space] = true;
      EmptyTable[Tab] = true;
      EmptyTable[Lf] = true;
      EmptyTable[Cr] = true;

      BlankTable[Space] = true;
      BlankTable[Tab] = true;

      FlowSymbolTable[','] = true;
      FlowSymbolTable['['] = true;
      FlowSymbolTable[']'] = true;
      FlowSymbolTable['{'] = true;
      FlowSymbolTable['}'] = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAlphaNumericDashOrUnderscore(byte code) =>
        IsNumber(code) || IsAlphabet(code) || code is (byte)'_' or (byte)'-';

    // Spec: https://yaml.org/spec/1.2.2/#rule-ns-word-char
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsWordChar(byte code) =>
        IsNumber(code) || IsAlphabet(code) || code is (byte)'-';

    // Spec: https://yaml.org/spec/1.2.2/#rule-ns-uri-char
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsUriChar(byte code) => code is
        >= (byte)'0' and <= (byte)'9' or
        >= (byte)'A' and <= (byte)'Z' or
        >= (byte)'a' and <= (byte)'z' or
        (byte)'-' or
        (byte)'#' or
        (byte)';' or
        (byte)'/' or
        (byte)'?' or
        (byte)':' or
        (byte)'@' or
        (byte)'&' or
        (byte)'=' or
        (byte)'+' or
        (byte)'$' or
        (byte)',' or
        (byte)'_' or
        (byte)'.' or
        (byte)'!' or
        (byte)'~' or
        (byte)'*' or
        (byte)'\'' or
        (byte)'(' or
        (byte)')' or
        (byte)'[' or
        (byte)']';

    // Spec: https://yaml.org/spec/1.2.2/#rule-ns-tag-char
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsTagChar(byte code) => code is
        >= (byte)'0' and <= (byte)'9' or
        >= (byte)'A' and <= (byte)'Z' or
        >= (byte)'a' and <= (byte)'z' or
        (byte)'-' or
        (byte)'#' or
        (byte)';' or
        (byte)'/' or
        (byte)'?' or
        (byte)':' or
        (byte)'@' or
        (byte)'&' or
        (byte)'=' or
        (byte)'+' or
        (byte)'$' or
        // (byte)',' or
        (byte)'_' or
        (byte)'.' or
        // (byte)'!' or
        (byte)'~' or
        (byte)'*' or
        (byte)'\'' // or
                   // (byte)'(' or
                   // (byte)')' or
                   // (byte)'[' or
                   // (byte)']'
        ;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAscii(byte code) => code <= '\x7F';

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNumber(byte c) => (byte)((c | 0x20) - (byte)'0') < 10;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEmpty(byte code) => EmptyTable[code];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBlank(byte code) => BlankTable[code];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLineBreak(byte code) => code is Lf or Cr;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAlphabet(byte c) => (byte)((c | 0x20) - (byte)'a') < 26;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsHexAlphabet(byte c) => (byte)((c | 0x20) - (byte)'a') < 6;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsHex(byte code) => IsNumber(code) || IsHexAlphabet(code);

    // Spec: https://yaml.org/spec/1.2.2/#rule-c-flow-indicator
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAnyFlowSymbol(byte code) => FlowSymbolTable[code];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte AsHex(byte code)
    {
      var x = code - (byte)'0';
      if ((uint)x <= 9)
      {
        return (byte)x;
      }
      x = (code | 0x20) - (byte)'a';
      if ((uint)x <= 5)
      {
        return (byte)(x + 10);
      }
      throw new InvalidOperationException();
    }
  }
}
