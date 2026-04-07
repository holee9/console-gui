using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace HnVue.UI.Components.Medical
{
    /// <summary>
    /// Live acquisition preview with exposure indicators and safety controls.
    /// CRITICAL PATH: This component controls radiation exposure.
    /// </summary>
    public class AcquisitionPreview : ContentControl
    {
        static AcquisitionPreview()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AcquisitionPreview),
                new FrameworkPropertyMetadata(typeof(AcquisitionPreview)));
        }

        public static readonly DependencyProperty LiveImageProperty =
            DependencyProperty.Register(
                nameof(LiveImage),
                typeof(BitmapSource),
                typeof(AcquisitionPreview),
                new PropertyMetadata(null));

        /// <summary>
        /// Current live image from the detector.
        /// </summary>
        public BitmapSource LiveImage
        {
            get => (BitmapSource)GetValue(LiveImageProperty);
            set => SetValue(LiveImageProperty, value);
        }

        public static readonly DependencyProperty IsAcquiringProperty =
            DependencyProperty.Register(
                nameof(IsAcquiring),
                typeof(bool),
                typeof(AcquisitionPreview),
                new PropertyMetadata(false, OnIsAcquiringChanged));

        /// <summary>
        /// Indicates an active acquisition is in progress.
        /// Shows visual and audible indicators when true.
        /// </summary>
        public bool IsAcquiring
        {
            get => (bool)GetValue(IsAcquiringProperty);
            set => SetValue(IsAcquiringProperty, value);
        }

        private static void OnIsAcquiringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AcquisitionPreview preview && e.NewValue is bool acquiring)
            {
                if (acquiring)
                {
                    preview.OnAcquisitionStarted();
                }
                else
                {
                    preview.OnAcquisitionEnded();
                }
            }
        }

        public static readonly DependencyProperty ExposureInProgressProperty =
            DependencyProperty.Register(
                nameof(ExposureInProgress),
                typeof(bool),
                typeof(AcquisitionPreview),
                new PropertyMetadata(false));

        /// <summary>
        /// Indicates radiation exposure is active.
        /// Must be clearly visible for safety compliance.
        /// </summary>
        public bool ExposureInProgress
        {
            get => (bool)GetValue(ExposureInProgressProperty);
            set => SetValue(ExposureInProgressProperty, value);
        }

        public static readonly DependencyProperty ExposureCountProperty =
            DependencyProperty.Register(
                nameof(ExposureCount),
                typeof(int),
                typeof(AcquisitionPreview),
                new PropertyMetadata(0));

        public int ExposureCount
        {
            get => (int)GetValue(ExposureCountProperty);
            set => SetValue(ExposureCountProperty, value);
        }

        public static readonly DependencyProperty BodyPartProperty =
            DependencyProperty.Register(
                nameof(BodyPart),
                typeof(BodyPart),
                typeof(AcquisitionPreview),
                new PropertyMetadata(BodyPart.Unknown));

        public BodyPart BodyPart
        {
            get => (BodyPart)GetValue(BodyPartProperty);
            set => SetValue(BodyPartProperty, value);
        }

        public static readonly DependencyProperty ViewPositionProperty =
            DependencyProperty.Register(
                nameof(ViewPosition),
                typeof(ViewPosition),
                typeof(AcquisitionPreview),
                new PropertyMetadata(ViewPosition.Unknown));

        public ViewPosition ViewPosition
        {
            get => (ViewPosition)GetValue(ViewPositionProperty);
            set => SetValue(ViewPositionProperty, value);
        }

        protected virtual void OnAcquisitionStarted()
        {
            // Trigger safety indicators
            // Play audible alert
        }

        protected virtual void OnAcquisitionEnded()
        {
            // Clear safety indicators
        }
    }

    /// <summary>
    /// DICOM-defined body parts for acquisition targeting.
    /// </summary>
    public enum BodyPart
    {
        Unknown,
        Chest,
        Abdomen,
        Skull,
        Spine,
        Extremity,
        Hand,
        Foot,
        Pelvis
    }

    /// <summary>
    /// Radiographic view positions.
    /// </summary>
    public enum ViewPosition
    {
        Unknown,
        AP,  // Anteroposterior
        PA,  // Posteroanterior
        LAT,  // Lateral
        OBL,  // Oblique
        AXIAL
    }
}
