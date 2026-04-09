using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.UI.Converters;
using Xunit;

namespace HnVue.UI.Tests;

/// <summary>
/// Unit tests for all Converter classes in HnVue.UI.Converters namespace.
/// Covers Convert() and ConvertBack() methods with boundary values.
/// </summary>
public class ConverterTests
{
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;
    private static readonly Type TargetType = typeof(object);

    // ====================================================================
    // InverseBoolConverter
    // ====================================================================

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "InverseBoolConverter")]
    public void InverseBoolConverter_Convert_True_Returns_False()
    {
        var converter = new InverseBoolConverter();
        converter.Convert(true, TargetType, null, Culture).Should().Be(false);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "InverseBoolConverter")]
    public void InverseBoolConverter_Convert_False_Returns_True()
    {
        var converter = new InverseBoolConverter();
        converter.Convert(false, TargetType, null, Culture).Should().Be(true);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "InverseBoolConverter")]
    public void InverseBoolConverter_Convert_NonBool_Returns_True()
    {
        var converter = new InverseBoolConverter();
        converter.Convert(null, TargetType, null, Culture).Should().Be(true);
        converter.Convert("string", TargetType, null, Culture).Should().Be(true);
        converter.Convert(42, TargetType, null, Culture).Should().Be(true);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "InverseBoolConverter")]
    public void InverseBoolConverter_ConvertBack_True_Returns_False()
    {
        var converter = new InverseBoolConverter();
        converter.ConvertBack(true, TargetType, null, Culture).Should().Be(false);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "InverseBoolConverter")]
    public void InverseBoolConverter_ConvertBack_False_Returns_True()
    {
        var converter = new InverseBoolConverter();
        converter.ConvertBack(false, TargetType, null, Culture).Should().Be(true);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "InverseBoolConverter")]
    public void InverseBoolConverter_ConvertBack_NonBool_Returns_True()
    {
        var converter = new InverseBoolConverter();
        converter.ConvertBack(null, TargetType, null, Culture).Should().Be(true);
    }

    // ====================================================================
    // NullToVisibilityConverter
    // ====================================================================

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "NullToVisibilityConverter")]
    public void NullToVisibilityConverter_Convert_Null_Returns_Collapsed()
    {
        var converter = new NullToVisibilityConverter();
        converter.Convert(null, TargetType, null, Culture).Should().Be(Visibility.Collapsed);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "NullToVisibilityConverter")]
    public void NullToVisibilityConverter_Convert_String_Returns_Visible()
    {
        var converter = new NullToVisibilityConverter();
        converter.Convert("value", TargetType, null, Culture).Should().Be(Visibility.Visible);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "NullToVisibilityConverter")]
    public void NullToVisibilityConverter_Convert_Object_Returns_Visible()
    {
        var converter = new NullToVisibilityConverter();
        converter.Convert(new object(), TargetType, null, Culture).Should().Be(Visibility.Visible);
        converter.Convert(0, TargetType, null, Culture).Should().Be(Visibility.Visible);
        converter.Convert(false, TargetType, null, Culture).Should().Be(Visibility.Visible);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "NullToVisibilityConverter")]
    public void NullToVisibilityConverter_ConvertBack_Throws_NotSupportedException()
    {
        var converter = new NullToVisibilityConverter();
        Action act = () => converter.ConvertBack(Visibility.Visible, TargetType, null, Culture);
        act.Should().Throw<NotSupportedException>();
    }

    // ====================================================================
    // InvertedBoolToVisibilityConverter
    // ====================================================================

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "InvertedBoolToVisibilityConverter")]
    public void InvertedBoolToVisibilityConverter_Convert_False_Returns_Visible()
    {
        var converter = new InvertedBoolToVisibilityConverter();
        converter.Convert(false, TargetType, null, Culture).Should().Be(Visibility.Visible);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "InvertedBoolToVisibilityConverter")]
    public void InvertedBoolToVisibilityConverter_Convert_True_Returns_Collapsed()
    {
        var converter = new InvertedBoolToVisibilityConverter();
        converter.Convert(true, TargetType, null, Culture).Should().Be(Visibility.Collapsed);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "InvertedBoolToVisibilityConverter")]
    public void InvertedBoolToVisibilityConverter_Convert_Null_Returns_Collapsed()
    {
        var converter = new InvertedBoolToVisibilityConverter();
        // null is not false, so returns Collapsed
        converter.Convert(null, TargetType, null, Culture).Should().Be(Visibility.Collapsed);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "InvertedBoolToVisibilityConverter")]
    public void InvertedBoolToVisibilityConverter_ConvertBack_Collapsed_Returns_True()
    {
        var converter = new InvertedBoolToVisibilityConverter();
        converter.ConvertBack(Visibility.Collapsed, TargetType, null, Culture).Should().Be(true);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "InvertedBoolToVisibilityConverter")]
    public void InvertedBoolToVisibilityConverter_ConvertBack_Visible_Returns_False()
    {
        var converter = new InvertedBoolToVisibilityConverter();
        converter.ConvertBack(Visibility.Visible, TargetType, null, Culture).Should().Be(false);
    }

    // ====================================================================
    // NullToCollapsedConverter
    // ====================================================================

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "NullToCollapsedConverter")]
    public void NullToCollapsedConverter_Convert_Null_Returns_Visible()
    {
        var converter = new NullToCollapsedConverter();
        // Null => Visible (inverted null check)
        converter.Convert(null, TargetType, null, Culture).Should().Be(Visibility.Visible);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "NullToCollapsedConverter")]
    public void NullToCollapsedConverter_Convert_NonNull_Returns_Collapsed()
    {
        var converter = new NullToCollapsedConverter();
        converter.Convert("value", TargetType, null, Culture).Should().Be(Visibility.Collapsed);
        converter.Convert(new object(), TargetType, null, Culture).Should().Be(Visibility.Collapsed);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "NullToCollapsedConverter")]
    public void NullToCollapsedConverter_ConvertBack_Throws_NotImplementedException()
    {
        var converter = new NullToCollapsedConverter();
        Action act = () => converter.ConvertBack(null, TargetType, null, Culture);
        act.Should().Throw<NotImplementedException>();
    }

    // ====================================================================
    // CountToVisibilityConverter
    // ====================================================================

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "CountToVisibilityConverter")]
    public void CountToVisibilityConverter_Convert_PositiveCount_Returns_Visible()
    {
        var converter = new CountToVisibilityConverter();
        converter.Convert(1, TargetType, null, Culture).Should().Be(Visibility.Visible);
        converter.Convert(100, TargetType, null, Culture).Should().Be(Visibility.Visible);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "CountToVisibilityConverter")]
    public void CountToVisibilityConverter_Convert_ZeroCount_Returns_Collapsed()
    {
        var converter = new CountToVisibilityConverter();
        converter.Convert(0, TargetType, null, Culture).Should().Be(Visibility.Collapsed);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "CountToVisibilityConverter")]
    public void CountToVisibilityConverter_Convert_NegativeCount_Returns_Collapsed()
    {
        var converter = new CountToVisibilityConverter();
        // Negative count <= 0 => Collapsed
        converter.Convert(-1, TargetType, null, Culture).Should().Be(Visibility.Collapsed);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "CountToVisibilityConverter")]
    public void CountToVisibilityConverter_Convert_NonInt_Returns_Collapsed()
    {
        var converter = new CountToVisibilityConverter();
        converter.Convert(null, TargetType, null, Culture).Should().Be(Visibility.Collapsed);
        converter.Convert("5", TargetType, null, Culture).Should().Be(Visibility.Collapsed);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "CountToVisibilityConverter")]
    public void CountToVisibilityConverter_ConvertBack_Throws_NotImplementedException()
    {
        var converter = new CountToVisibilityConverter();
        Action act = () => converter.ConvertBack(null, TargetType, null, Culture);
        act.Should().Throw<NotImplementedException>();
    }

    // ====================================================================
    // StringToVisibilityConverter
    // ====================================================================

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "StringToVisibilityConverter")]
    public void StringToVisibilityConverter_Convert_NonEmptyString_Returns_Visible()
    {
        var converter = new StringToVisibilityConverter();
        converter.Convert("hello", TargetType, null, Culture).Should().Be(Visibility.Visible);
        converter.Convert("a", TargetType, null, Culture).Should().Be(Visibility.Visible);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "StringToVisibilityConverter")]
    public void StringToVisibilityConverter_Convert_EmptyString_Returns_Collapsed()
    {
        var converter = new StringToVisibilityConverter();
        converter.Convert("", TargetType, null, Culture).Should().Be(Visibility.Collapsed);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "StringToVisibilityConverter")]
    public void StringToVisibilityConverter_Convert_WhitespaceString_Returns_Collapsed()
    {
        var converter = new StringToVisibilityConverter();
        converter.Convert("   ", TargetType, null, Culture).Should().Be(Visibility.Collapsed);
        converter.Convert("\t\n", TargetType, null, Culture).Should().Be(Visibility.Collapsed);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "StringToVisibilityConverter")]
    public void StringToVisibilityConverter_Convert_Null_Returns_Collapsed()
    {
        var converter = new StringToVisibilityConverter();
        converter.Convert(null, TargetType, null, Culture).Should().Be(Visibility.Collapsed);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "StringToVisibilityConverter")]
    public void StringToVisibilityConverter_Convert_NonString_Returns_Collapsed()
    {
        var converter = new StringToVisibilityConverter();
        converter.Convert(123, TargetType, null, Culture).Should().Be(Visibility.Collapsed);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "StringToVisibilityConverter")]
    public void StringToVisibilityConverter_ConvertBack_Throws_NotImplementedException()
    {
        var converter = new StringToVisibilityConverter();
        Action act = () => converter.ConvertBack(null, TargetType, null, Culture);
        act.Should().Throw<NotImplementedException>();
    }

    // ====================================================================
    // ActiveTabToVisibilityConverter
    // ====================================================================

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "ActiveTabToVisibilityConverter")]
    public void ActiveTabToVisibilityConverter_Convert_MatchingTab_Returns_Visible()
    {
        var converter = new ActiveTabToVisibilityConverter();
        converter.Convert("Settings", TargetType, "Settings", Culture).Should().Be(Visibility.Visible);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "ActiveTabToVisibilityConverter")]
    public void ActiveTabToVisibilityConverter_Convert_CaseInsensitiveMatch_Returns_Visible()
    {
        var converter = new ActiveTabToVisibilityConverter();
        converter.Convert("settings", TargetType, "Settings", Culture).Should().Be(Visibility.Visible);
        converter.Convert("SETTINGS", TargetType, "settings", Culture).Should().Be(Visibility.Visible);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "ActiveTabToVisibilityConverter")]
    public void ActiveTabToVisibilityConverter_Convert_NonMatchingTab_Returns_Collapsed()
    {
        var converter = new ActiveTabToVisibilityConverter();
        converter.Convert("Dashboard", TargetType, "Settings", Culture).Should().Be(Visibility.Collapsed);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "ActiveTabToVisibilityConverter")]
    public void ActiveTabToVisibilityConverter_Convert_NullValue_Returns_Collapsed()
    {
        var converter = new ActiveTabToVisibilityConverter();
        converter.Convert(null, TargetType, "Settings", Culture).Should().Be(Visibility.Collapsed);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "ActiveTabToVisibilityConverter")]
    public void ActiveTabToVisibilityConverter_Convert_NullParameter_Returns_Collapsed()
    {
        var converter = new ActiveTabToVisibilityConverter();
        converter.Convert("Settings", TargetType, null, Culture).Should().Be(Visibility.Collapsed);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "ActiveTabToVisibilityConverter")]
    public void ActiveTabToVisibilityConverter_ConvertBack_Throws_NotSupportedException()
    {
        var converter = new ActiveTabToVisibilityConverter();
        Action act = () => converter.ConvertBack(null, TargetType, null, Culture);
        act.Should().Throw<NotSupportedException>();
    }

    // ====================================================================
    // StringEqualityToBoolConverter
    // ====================================================================

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "StringEqualityToBoolConverter")]
    public void StringEqualityToBoolConverter_Convert_MatchingStrings_Returns_True()
    {
        var converter = new StringEqualityToBoolConverter();
        converter.Convert("Hello", TargetType, "Hello", Culture).Should().Be(true);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "StringEqualityToBoolConverter")]
    public void StringEqualityToBoolConverter_Convert_CaseInsensitiveMatch_Returns_True()
    {
        var converter = new StringEqualityToBoolConverter();
        converter.Convert("hello", TargetType, "Hello", Culture).Should().Be(true);
        converter.Convert("HELLO", TargetType, "hello", Culture).Should().Be(true);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "StringEqualityToBoolConverter")]
    public void StringEqualityToBoolConverter_Convert_DifferentStrings_Returns_False()
    {
        var converter = new StringEqualityToBoolConverter();
        converter.Convert("Hello", TargetType, "World", Culture).Should().Be(false);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "StringEqualityToBoolConverter")]
    public void StringEqualityToBoolConverter_Convert_NullValue_Returns_False()
    {
        var converter = new StringEqualityToBoolConverter();
        converter.Convert(null, TargetType, "Hello", Culture).Should().Be(false);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "StringEqualityToBoolConverter")]
    public void StringEqualityToBoolConverter_Convert_NullParameter_Returns_False()
    {
        var converter = new StringEqualityToBoolConverter();
        converter.Convert("Hello", TargetType, null, Culture).Should().Be(false);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "StringEqualityToBoolConverter")]
    public void StringEqualityToBoolConverter_Convert_BothNull_Returns_False()
    {
        var converter = new StringEqualityToBoolConverter();
        converter.Convert(null, TargetType, null, Culture).Should().Be(false);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "StringEqualityToBoolConverter")]
    public void StringEqualityToBoolConverter_ConvertBack_Throws_NotSupportedException()
    {
        var converter = new StringEqualityToBoolConverter();
        Action act = () => converter.ConvertBack(null, TargetType, null, Culture);
        act.Should().Throw<NotSupportedException>();
    }

    // ====================================================================
    // MultiBoolAndConverter
    // ====================================================================

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "MultiBoolAndConverter")]
    public void MultiBoolAndConverter_Convert_AllTrue_Returns_True()
    {
        var converter = new MultiBoolAndConverter();
        var values = new object[] { true, true, true };
        converter.Convert(values, TargetType, null, Culture).Should().Be(true);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "MultiBoolAndConverter")]
    public void MultiBoolAndConverter_Convert_AnyFalse_Returns_False()
    {
        var converter = new MultiBoolAndConverter();
        var values = new object[] { true, false, true };
        converter.Convert(values, TargetType, null, Culture).Should().Be(false);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "MultiBoolAndConverter")]
    public void MultiBoolAndConverter_Convert_AllFalse_Returns_False()
    {
        var converter = new MultiBoolAndConverter();
        var values = new object[] { false, false };
        converter.Convert(values, TargetType, null, Culture).Should().Be(false);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "MultiBoolAndConverter")]
    public void MultiBoolAndConverter_Convert_EmptyArray_Returns_True()
    {
        // All() on empty returns true (vacuous truth)
        var converter = new MultiBoolAndConverter();
        var values = Array.Empty<object>();
        converter.Convert(values, TargetType, null, Culture).Should().Be(true);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "MultiBoolAndConverter")]
    public void MultiBoolAndConverter_Convert_NonBoolValuesFiltered_ReturnsTrue()
    {
        // OfType<bool> filters out non-bool; all filtered = true (vacuous)
        var converter = new MultiBoolAndConverter();
        var values = new object[] { "string", 42, null! };
        converter.Convert(values, TargetType, null, Culture).Should().Be(true);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "MultiBoolAndConverter")]
    public void MultiBoolAndConverter_ConvertBack_Throws_NotImplementedException()
    {
        var converter = new MultiBoolAndConverter();
        Action act = () => converter.ConvertBack(null, Array.Empty<Type>(), null, Culture);
        act.Should().Throw<NotImplementedException>();
    }

    // ====================================================================
    // MultiBoolOrConverter
    // ====================================================================

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "MultiBoolOrConverter")]
    public void MultiBoolOrConverter_Convert_AnyTrue_Returns_True()
    {
        var converter = new MultiBoolOrConverter();
        var values = new object[] { false, true, false };
        converter.Convert(values, TargetType, null, Culture).Should().Be(true);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "MultiBoolOrConverter")]
    public void MultiBoolOrConverter_Convert_AllTrue_Returns_True()
    {
        var converter = new MultiBoolOrConverter();
        var values = new object[] { true, true };
        converter.Convert(values, TargetType, null, Culture).Should().Be(true);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "MultiBoolOrConverter")]
    public void MultiBoolOrConverter_Convert_AllFalse_Returns_False()
    {
        var converter = new MultiBoolOrConverter();
        var values = new object[] { false, false, false };
        converter.Convert(values, TargetType, null, Culture).Should().Be(false);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "MultiBoolOrConverter")]
    public void MultiBoolOrConverter_Convert_EmptyArray_Returns_False()
    {
        // Any() on empty returns false
        var converter = new MultiBoolOrConverter();
        var values = Array.Empty<object>();
        converter.Convert(values, TargetType, null, Culture).Should().Be(false);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "MultiBoolOrConverter")]
    public void MultiBoolOrConverter_ConvertBack_Throws_NotImplementedException()
    {
        var converter = new MultiBoolOrConverter();
        Action act = () => converter.ConvertBack(null, Array.Empty<Type>(), null, Culture);
        act.Should().Throw<NotImplementedException>();
    }

    // ====================================================================
    // StatusToBrushConverter
    // ====================================================================

    private enum TestStatus { Safe, Warning, Error, Info, Online, Offline, Busy, Blocked, Unknown }

    [Theory]
    [InlineData(TestStatus.Safe)]
    [InlineData(TestStatus.Warning)]
    [InlineData(TestStatus.Error)]
    [InlineData(TestStatus.Info)]
    [InlineData(TestStatus.Online)]
    [InlineData(TestStatus.Offline)]
    [InlineData(TestStatus.Busy)]
    [InlineData(TestStatus.Blocked)]
    [Trait("Category", "Converters")]
    [Trait("Converter", "StatusToBrushConverter")]
    public void StatusToBrushConverter_Convert_KnownStatus_Returns_CorrectBrush(TestStatus status)
    {
        var converter = new StatusToBrushConverter();
        var result = converter.Convert(status, TargetType, null, Culture);
        result.Should().BeOfType<SolidColorBrush>();
        result.Should().NotBe(Brushes.Gray);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "StatusToBrushConverter")]
    public void StatusToBrushConverter_Convert_UnknownStatus_Returns_Gray()
    {
        var converter = new StatusToBrushConverter();
        var result = converter.Convert(TestStatus.Unknown, TargetType, null, Culture);
        result.Should().Be(Brushes.Gray);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "StatusToBrushConverter")]
    public void StatusToBrushConverter_Convert_NonEnum_Returns_Gray()
    {
        var converter = new StatusToBrushConverter();
        converter.Convert(null, TargetType, null, Culture).Should().Be(Brushes.Gray);
        converter.Convert("Safe", TargetType, null, Culture).Should().Be(Brushes.Gray);
        converter.Convert(42, TargetType, null, Culture).Should().Be(Brushes.Gray);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "StatusToBrushConverter")]
    public void StatusToBrushConverter_ConvertBack_Throws_NotImplementedException()
    {
        var converter = new StatusToBrushConverter();
        Action act = () => converter.ConvertBack(null, TargetType, null, Culture);
        act.Should().Throw<NotImplementedException>();
    }

    // ====================================================================
    // SafeStateToColorConverter
    // ====================================================================

    [Theory]
    [InlineData(SafeState.Idle, 0x00, 0xC8, 0x53)]
    [InlineData(SafeState.Warning, 0xFF, 0xD6, 0x00)]
    [InlineData(SafeState.Degraded, 0xFF, 0x6D, 0x00)]
    [InlineData(SafeState.Blocked, 0xE6, 0x5C, 0x00)]
    [InlineData(SafeState.Emergency, 0xD5, 0x00, 0x00)]
    [Trait("Category", "Converters")]
    [Trait("Converter", "SafeStateToColorConverter")]
    public void SafeStateToColorConverter_Convert_KnownState_Returns_CorrectColor(
        SafeState state, byte expectedR, byte expectedG, byte expectedB)
    {
        var converter = new SafeStateToColorConverter();
        var result = converter.Convert(state, TargetType, null, Culture);

        result.Should().BeOfType<SolidColorBrush>();
        var brush = (SolidColorBrush)result;
        brush.Color.R.Should().Be(expectedR);
        brush.Color.G.Should().Be(expectedG);
        brush.Color.B.Should().Be(expectedB);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "SafeStateToColorConverter")]
    public void SafeStateToColorConverter_Convert_NonSafeState_Returns_Gray()
    {
        var converter = new SafeStateToColorConverter();
        converter.Convert(null, TargetType, null, Culture).Should().Be(Brushes.Gray);
        converter.Convert("Idle", TargetType, null, Culture).Should().Be(Brushes.Gray);
        converter.Convert(99, TargetType, null, Culture).Should().Be(Brushes.Gray);
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "SafeStateToColorConverter")]
    public void SafeStateToColorConverter_ConvertBack_Throws_NotSupportedException()
    {
        var converter = new SafeStateToColorConverter();
        Action act = () => converter.ConvertBack(null, TargetType, null, Culture);
        act.Should().Throw<NotSupportedException>();
    }

    [Fact]
    [Trait("Category", "Converters")]
    [Trait("Converter", "SafeStateToColorConverter")]
    public void SafeStateToColorConverter_Convert_AllStates_AreDifferentColors()
    {
        var converter = new SafeStateToColorConverter();
        var states = new[] { SafeState.Idle, SafeState.Warning, SafeState.Degraded, SafeState.Blocked, SafeState.Emergency };
        var colors = states
            .Select(s => ((SolidColorBrush)converter.Convert(s, TargetType, null, Culture)).Color)
            .ToList();

        // Each state should have a distinct color
        colors.Distinct().Should().HaveCount(5);
    }
}
