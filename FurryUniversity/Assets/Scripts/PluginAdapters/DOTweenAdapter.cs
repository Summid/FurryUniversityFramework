using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace SFramework.Adapter
{
    /// <summary>
    /// 包装DOTween API，方便更换DOTween版本后修改API
    /// </summary>
    public static class DOTweenAdapter
    {
        #region UGUI

        #region CanvasGroup

        /// <summary>Tweens a CanvasGroup's alpha color to the given value.
        /// Also stores the canvasGroup as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static TweenerCore<float, float, FloatOptions> DOFadeByAdapter(this CanvasGroup target, float endValue, float duration)
        {
            return target.DOFade(endValue, duration);
        }

        #endregion

        #region Graphic

        /// <summary>Tweens an Graphic's color to the given value.
        /// Also stores the image as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static TweenerCore<Color, Color, ColorOptions> DOColorByAdapter(this Graphic target, Color endValue, float duration)
        {
            return target.DOColor(endValue, duration);
        }

        /// <summary>Tweens an Graphic's alpha color to the given value.
        /// Also stores the image as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static TweenerCore<Color, Color, ColorOptions> DOFadeByAdapter(this Graphic target, float endValue, float duration)
        {
            return target.DOFade(endValue, duration);
        }

        #endregion

        #region Image

        /// <summary>Tweens an Image's color to the given value.
        /// Also stores the image as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static TweenerCore<Color, Color, ColorOptions> DoColorByAdapter(this Image target, Color endValue, float duration)
        {
            return target.DOColor(endValue, duration);
        }

        /// <summary>Tweens an Image's alpha color to the given value.
        /// Also stores the image as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static TweenerCore<Color, Color, ColorOptions> DOFadeByAdapter(this Image target, float endValue, float duration)
        {
            return target.DOFade(endValue, duration);
        }

        /// <summary>Tweens an Image's fillAmount to the given value.
        /// Also stores the image as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach (0 to 1)</param><param name="duration">The duration of the tween</param>
        public static TweenerCore<float, float, FloatOptions> DOFillAmountByAdapter(this Image target, float endValue, float duration)
        {
            return target.DOFillAmount(endValue, duration);
        }

        /// <summary>Tweens an Image's colors using the given gradient
        /// (NOTE 1: only uses the colors of the gradient, not the alphas - NOTE 2: creates a Sequence, not a Tweener).
        /// Also stores the image as the tween's target so it can be used for filtered operations</summary>
        /// <param name="gradient">The gradient to use</param><param name="duration">The duration of the tween</param>
        public static Sequence DOGradientColorByAdapter(this Image target, Gradient gradient, float duration)
        {
            return target.DOGradientColor(gradient, duration);
        }

        #endregion

        #region LayoutElement

        /// <summary>Tweens an LayoutElement's flexibleWidth/Height to the given value.
        /// Also stores the LayoutElement as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        /// <param name="snapping">If TRUE the tween will smoothly snap all values to integers</param>
        public static TweenerCore<Vector2, Vector2, VectorOptions> DOFlexibleSizeByAdapter(this LayoutElement target, Vector2 endValue, float duration, bool snapping = false)
        {
            return target.DOFlexibleSize(endValue, duration, snapping);
        }

        /// <summary>Tweens an LayoutElement's minWidth/Height to the given value.
        /// Also stores the LayoutElement as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        /// <param name="snapping">If TRUE the tween will smoothly snap all values to integers</param>
        public static TweenerCore<Vector2, Vector2, VectorOptions> DOMinSizeByAdapter(this LayoutElement target, Vector2 endValue, float duration, bool snapping = false)
        {
            return target.DOMinSize(endValue, duration, snapping);
        }

        /// <summary>Tweens an LayoutElement's preferredWidth/Height to the given value.
        /// Also stores the LayoutElement as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        /// <param name="snapping">If TRUE the tween will smoothly snap all values to integers</param>
        public static TweenerCore<Vector2, Vector2, VectorOptions> DOPreferredSizeByAdapter(this LayoutElement target, Vector2 endValue, float duration, bool snapping = false)
        {
            return target.DOPreferredSize(endValue, duration, snapping);
        }

        #endregion

        #region Outline

        /// <summary>Tweens a Outline's effectColor to the given value.
        /// Also stores the Outline as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static TweenerCore<Color, Color, ColorOptions> DOColorByAdapter(this Outline target, Color endValue, float duration)
        {
            return target.DOColor(endValue, duration);
        }

        /// <summary>Tweens a Outline's effectColor alpha to the given value.
        /// Also stores the Outline as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static TweenerCore<Color, Color, ColorOptions> DOFadeByAdapter(this Outline target, float endValue, float duration)
        {
            return target.DOFade(endValue, duration);
        }

        /// <summary>Tweens a Outline's effectDistance to the given value.
        /// Also stores the Outline as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static TweenerCore<Vector2, Vector2, VectorOptions> DOScaleByAdapter(this Outline target, Vector2 endValue, float duration)
        {
            return target.DOScale(endValue, duration);
        }

        #endregion

        #region RectTransform

        /// <summary>Tweens a RectTransform's anchoredPosition to the given value.
        /// Also stores the RectTransform as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        /// <param name="snapping">If TRUE the tween will smoothly snap all values to integers</param>
        public static TweenerCore<Vector2, Vector2, VectorOptions> DOAnchorPosByAdapter(this RectTransform target, Vector2 endValue, float duration, bool snapping = false)
        {
            return target.DOAnchorPos(endValue, duration, snapping);
        }
        /// <summary>Tweens a RectTransform's anchoredPosition X to the given value.
        /// Also stores the RectTransform as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        /// <param name="snapping">If TRUE the tween will smoothly snap all values to integers</param>
        public static TweenerCore<Vector2, Vector2, VectorOptions> DOAnchorPosXByAdapter(this RectTransform target, float endValue, float duration, bool snapping = false)
        {
            return target.DOAnchorPosX(endValue, duration, snapping);
        }
        /// <summary>Tweens a RectTransform's anchoredPosition Y to the given value.
        /// Also stores the RectTransform as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        /// <param name="snapping">If TRUE the tween will smoothly snap all values to integers</param>
        public static TweenerCore<Vector2, Vector2, VectorOptions> DOAnchorPosYByAdapter(this RectTransform target, float endValue, float duration, bool snapping = false)
        {
            return target.DOAnchorPosY(endValue, duration, snapping);
        }

        /// <summary>Tweens a RectTransform's anchoredPosition3D to the given value.
        /// Also stores the RectTransform as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        /// <param name="snapping">If TRUE the tween will smoothly snap all values to integers</param>
        public static TweenerCore<Vector3, Vector3, VectorOptions> DOAnchorPos3DByAdapter(this RectTransform target, Vector3 endValue, float duration, bool snapping = false)
        {
            return target.DOAnchorPos3D(endValue, duration, snapping);
        }
        /// <summary>Tweens a RectTransform's anchoredPosition3D X to the given value.
        /// Also stores the RectTransform as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        /// <param name="snapping">If TRUE the tween will smoothly snap all values to integers</param>
        public static TweenerCore<Vector3, Vector3, VectorOptions> DOAnchorPos3DXByAdapter(this RectTransform target, float endValue, float duration, bool snapping = false)
        {
            return target.DOAnchorPos3DX(endValue, duration, snapping);
        }
        /// <summary>Tweens a RectTransform's anchoredPosition3D Y to the given value.
        /// Also stores the RectTransform as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        /// <param name="snapping">If TRUE the tween will smoothly snap all values to integers</param>
        public static TweenerCore<Vector3, Vector3, VectorOptions> DOAnchorPos3DYByAdapter(this RectTransform target, float endValue, float duration, bool snapping = false)
        {
            return target.DOAnchorPos3DY(endValue, duration, snapping);

        }
        /// <summary>Tweens a RectTransform's anchoredPosition3D Z to the given value.
        /// Also stores the RectTransform as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        /// <param name="snapping">If TRUE the tween will smoothly snap all values to integers</param>
        public static TweenerCore<Vector3, Vector3, VectorOptions> DOAnchorPos3DZByAdapter(this RectTransform target, float endValue, float duration, bool snapping = false)
        {
            return target.DOAnchorPos3DZ(endValue, duration, snapping);
        }

        /// <summary>Tweens a RectTransform's anchorMax to the given value.
        /// Also stores the RectTransform as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        /// <param name="snapping">If TRUE the tween will smoothly snap all values to integers</param>
        public static TweenerCore<Vector2, Vector2, VectorOptions> DOAnchorMaxByAdapter(this RectTransform target, Vector2 endValue, float duration, bool snapping = false)
        {
            return target.DOAnchorMax(endValue, duration, snapping);
        }

        /// <summary>Tweens a RectTransform's anchorMin to the given value.
        /// Also stores the RectTransform as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        /// <param name="snapping">If TRUE the tween will smoothly snap all values to integers</param>
        public static TweenerCore<Vector2, Vector2, VectorOptions> DOAnchorMinByAdapter(this RectTransform target, Vector2 endValue, float duration, bool snapping = false)
        {
            return target.DOAnchorMin(endValue, duration, snapping);
        }

        /// <summary>Tweens a RectTransform's pivot to the given value.
        /// Also stores the RectTransform as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static TweenerCore<Vector2, Vector2, VectorOptions> DOPivotByAdapter(this RectTransform target, Vector2 endValue, float duration)
        {
            return target.DOPivot(endValue, duration);
        }
        /// <summary>Tweens a RectTransform's pivot X to the given value.
        /// Also stores the RectTransform as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static TweenerCore<Vector2, Vector2, VectorOptions> DOPivotXByAdapter(this RectTransform target, float endValue, float duration)
        {
            return target.DOPivotX(endValue, duration);
        }
        /// <summary>Tweens a RectTransform's pivot Y to the given value.
        /// Also stores the RectTransform as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static TweenerCore<Vector2, Vector2, VectorOptions> DOPivotYByAdapter(this RectTransform target, float endValue, float duration)
        {
            return target.DOPivotY(endValue, duration);
        }

        /// <summary>Tweens a RectTransform's sizeDelta to the given value.
        /// Also stores the RectTransform as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        /// <param name="snapping">If TRUE the tween will smoothly snap all values to integers</param>
        public static TweenerCore<Vector2, Vector2, VectorOptions> DOSizeDeltaByAdapter(this RectTransform target, Vector2 endValue, float duration, bool snapping = false)
        {
            return target.DOSizeDelta(endValue, duration, snapping);
        }

        /// <summary>Punches a RectTransform's anchoredPosition towards the given direction and then back to the starting one
        /// as if it was connected to the starting position via an elastic.
        /// Also stores the RectTransform as the tween's target so it can be used for filtered operations</summary>
        /// <param name="punch">The direction and strength of the punch (added to the RectTransform's current position)</param>
        /// <param name="duration">The duration of the tween</param>
        /// <param name="vibrato">Indicates how much will the punch vibrate</param>
        /// <param name="elasticity">Represents how much (0 to 1) the vector will go beyond the starting position when bouncing backwards.
        /// 1 creates a full oscillation between the punch direction and the opposite direction,
        /// while 0 oscillates only between the punch and the start position</param>
        /// <param name="snapping">If TRUE the tween will smoothly snap all values to integers</param>
        public static Tweener DOPunchAnchorPosByAdapter(this RectTransform target, Vector2 punch, float duration, int vibrato = 10, float elasticity = 1, bool snapping = false)
        {
            return target.DOPunchAnchorPos(punch, duration, vibrato, elasticity, snapping);
        }

        /// <summary>Shakes a RectTransform's anchoredPosition with the given values.
        /// Also stores the RectTransform as the tween's target so it can be used for filtered operations</summary>
        /// <param name="duration">The duration of the tween</param>
        /// <param name="strength">The shake strength</param>
        /// <param name="vibrato">Indicates how much will the shake vibrate</param>
        /// <param name="randomness">Indicates how much the shake will be random (0 to 180 - values higher than 90 kind of suck, so beware). 
        /// Setting it to 0 will shake along a single direction.</param>
        /// <param name="snapping">If TRUE the tween will smoothly snap all values to integers</param>
        /// <param name="fadeOut">If TRUE the shake will automatically fadeOut smoothly within the tween's duration, otherwise it will not</param>
        /// <param name="randomnessMode">Randomness mode</param>
        public static Tweener DOShakeAnchorPosByAdapter(this RectTransform target, float duration, float strength = 100, int vibrato = 10, float randomness = 90, bool snapping = false, bool fadeOut = true, ShakeRandomnessMode randomnessMode = ShakeRandomnessMode.Full)
        {
            return target.DOShakeAnchorPos(duration, strength, vibrato, randomness, snapping, fadeOut, randomnessMode);
        }

        /// <summary>Shakes a RectTransform's anchoredPosition with the given values.
        /// Also stores the RectTransform as the tween's target so it can be used for filtered operations</summary>
        /// <param name="duration">The duration of the tween</param>
        /// <param name="strength">The shake strength on each axis</param>
        /// <param name="vibrato">Indicates how much will the shake vibrate</param>
        /// <param name="randomness">Indicates how much the shake will be random (0 to 180 - values higher than 90 kind of suck, so beware). 
        /// Setting it to 0 will shake along a single direction.</param>
        /// <param name="snapping">If TRUE the tween will smoothly snap all values to integers</param>
        /// <param name="fadeOut">If TRUE the shake will automatically fadeOut smoothly within the tween's duration, otherwise it will not</param>
        /// <param name="randomnessMode">Randomness mode</param>
        public static Tweener DOShakeAnchorPosByAdapter(this RectTransform target, float duration, Vector2 strength, int vibrato = 10, float randomness = 90, bool snapping = false, bool fadeOut = true, ShakeRandomnessMode randomnessMode = ShakeRandomnessMode.Full)
        {
            return target.DOShakeAnchorPos(duration, strength, vibrato, randomness, snapping, fadeOut, randomnessMode);
        }

        #region Special

        /// <summary>Tweens a RectTransform's anchoredPosition to the given value, while also applying a jump effect along the Y axis.
        /// Returns a Sequence instead of a Tweener.
        /// Also stores the RectTransform as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param>
        /// <param name="jumpPower">Power of the jump (the max height of the jump is represented by this plus the final Y offset)</param>
        /// <param name="numJumps">Total number of jumps</param>
        /// <param name="duration">The duration of the tween</param>
        /// <param name="snapping">If TRUE the tween will smoothly snap all values to integers</param>
        public static Sequence DOJumpAnchorPosByAdapter(this RectTransform target, Vector2 endValue, float jumpPower, int numJumps, float duration, bool snapping = false)
        {
            return target.DOJumpAnchorPos(endValue, jumpPower, numJumps, duration, snapping);
        }

        #endregion

        #endregion

        #region ScrollRect

        /// <summary>Tweens a ScrollRect's horizontal/verticalNormalizedPosition to the given value.
        /// Also stores the ScrollRect as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        /// <param name="snapping">If TRUE the tween will smoothly snap all values to integers</param>
        public static Tweener DONormalizedPosByAdapter(this ScrollRect target, Vector2 endValue, float duration, bool snapping = false)
        {
            return target.DONormalizedPos(endValue, duration, snapping);
        }
        /// <summary>Tweens a ScrollRect's horizontalNormalizedPosition to the given value.
        /// Also stores the ScrollRect as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        /// <param name="snapping">If TRUE the tween will smoothly snap all values to integers</param>
        public static Tweener DOHorizontalNormalizedPosByAdapter(this ScrollRect target, float endValue, float duration, bool snapping = false)
        {
            return target.DOHorizontalNormalizedPos(endValue, duration, snapping);
        }
        /// <summary>Tweens a ScrollRect's verticalNormalizedPosition to the given value.
        /// Also stores the ScrollRect as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        /// <param name="snapping">If TRUE the tween will smoothly snap all values to integers</param>
        public static Tweener DOVerticalNormalizedPosByAdapter(this ScrollRect target, float endValue, float duration, bool snapping = false)
        {
            return target.DOVerticalNormalizedPos(endValue, duration, snapping);
        }

        #endregion

        #region Slider

        /// <summary>Tweens a Slider's value to the given value.
        /// Also stores the Slider as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        /// <param name="snapping">If TRUE the tween will smoothly snap all values to integers</param>
        public static TweenerCore<float, float, FloatOptions> DOValueByAdapter(this Slider target, float endValue, float duration, bool snapping = false)
        {
            return target.DOValue(endValue, duration, snapping);
        }

        #endregion

        #region Text

        /// <summary>Tweens a Text's color to the given value.
        /// Also stores the Text as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static TweenerCore<Color, Color, ColorOptions> DOColorByAdapter(this Text target, Color endValue, float duration)
        {
            return target.DOColor(endValue, duration);
        }

        /// <summary>
        /// Tweens a Text's text from one integer to another, with options for thousands separators
        /// </summary>
        /// <param name="fromValue">The value to start from</param>
        /// <param name="endValue">The end value to reach</param>
        /// <param name="duration">The duration of the tween</param>
        /// <param name="addThousandsSeparator">If TRUE (default) also adds thousands separators</param>
        /// <param name="culture">The <see cref="CultureInfo"/> to use (InvariantCulture if NULL)</param>
        public static TweenerCore<int, int, NoOptions> DOCounterByAdapter(
            this Text target, int fromValue, int endValue, float duration, bool addThousandsSeparator = true, CultureInfo culture = null
        )
        {
            return target.DOCounter(fromValue, endValue, duration, addThousandsSeparator, culture);
        }

        /// <summary>Tweens a Text's alpha color to the given value.
        /// Also stores the Text as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static TweenerCore<Color, Color, ColorOptions> DOFadeByAdapter(this Text target, float endValue, float duration)
        {
            return target.DOFade(endValue, duration);
        }

        /// <summary>Tweens a Text's text to the given value.
        /// Also stores the Text as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end string to tween to</param><param name="duration">The duration of the tween</param>
        /// <param name="richTextEnabled">If TRUE (default), rich text will be interpreted correctly while animated,
        /// otherwise all tags will be considered as normal text</param>
        /// <param name="scrambleMode">The type of scramble mode to use, if any</param>
        /// <param name="scrambleChars">A string containing the characters to use for scrambling.
        /// Use as many characters as possible (minimum 10) because DOTween uses a fast scramble mode which gives better results with more characters.
        /// Leave it to NULL (default) to use default ones</param>
        public static TweenerCore<string, string, StringOptions> DOTextByAdapter(this Text target, string endValue, float duration, bool richTextEnabled = true, ScrambleMode scrambleMode = ScrambleMode.None, string scrambleChars = null)
        {
            return target.DOText(endValue, duration, richTextEnabled, scrambleMode, scrambleChars);
        }

        #endregion

        #endregion

        #region SpriteRenderer

        /// <summary>Tweens a SpriteRenderer's color to the given value.
        /// Also stores the spriteRenderer as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static TweenerCore<Color, Color, ColorOptions> DOColorByAdapter(this SpriteRenderer target, Color endValue, float duration)
        {
            return target.DOColor(endValue, duration);
        }

        /// <summary>Tweens a Material's alpha color to the given value.
        /// Also stores the spriteRenderer as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static TweenerCore<Color, Color, ColorOptions> DOFadeByAdapter(this SpriteRenderer target, float endValue, float duration)
        {
            return target.DOFade(endValue, duration);
        }

        /// <summary>Tweens a SpriteRenderer's color using the given gradient
        /// (NOTE 1: only uses the colors of the gradient, not the alphas - NOTE 2: creates a Sequence, not a Tweener).
        /// Also stores the image as the tween's target so it can be used for filtered operations</summary>
        /// <param name="gradient">The gradient to use</param><param name="duration">The duration of the tween</param>
        public static Sequence DOGradientColorByAdapter(this SpriteRenderer target, Gradient gradient, float duration)
        {
            return target.DOGradientColor(gradient, duration);
        }

        #endregion

        #region Audio

        /// <summary>Tweens an AudioSource's volume to the given value.
        /// Also stores the AudioSource as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach (0 to 1)</param><param name="duration">The duration of the tween</param>
        public static TweenerCore<float, float, FloatOptions> DOFadeByAdapter(this AudioSource target, float endValue, float duration)
        {
            return target.DOFade(endValue, duration);
        }

        /// <summary>Tweens an AudioSource's pitch to the given value.
        /// Also stores the AudioSource as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static TweenerCore<float, float, FloatOptions> DOPitchByAdapter(this AudioSource target, float endValue, float duration)
        {
            return target.DOPitch(endValue, duration);
        }

        #endregion
    }
}