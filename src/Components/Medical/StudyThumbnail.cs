using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace HnVue.UI.Components.Medical
{
    /// <summary>
    /// Displays medical image study thumbnail with status indicators.
    /// Used in Worklist and Studylist for quick study identification.
    /// </summary>
    public class StudyThumbnail : ContentControl
    {
        static StudyThumbnail()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(StudyThumbnail),
                new FrameworkPropertyMetadata(typeof(StudyThumbnail)));
        }

        public static readonly DependencyProperty StudyInstanceUidProperty =
            DependencyProperty.Register(
                nameof(StudyInstanceUid),
                typeof(string),
                typeof(StudyThumbnail),
                new PropertyMetadata(string.Empty));

        /// <summary>
        /// DICOM Study Instance UID for unique identification.
        /// </summary>
        public string StudyInstanceUid
        {
            get => (string)GetValue(StudyInstanceUidProperty);
            set => SetValue(StudyInstanceUidProperty, value);
        }

        public static readonly DependencyProperty ThumbnailImageProperty =
            DependencyProperty.Register(
                nameof(ThumbnailImage),
                typeof(BitmapImage),
                typeof(StudyThumbnail),
                new PropertyMetadata(null));

        public BitmapImage ThumbnailImage
        {
            get => (BitmapImage)GetValue(ThumbnailImageProperty);
            set => SetValue(ThumbnailImageProperty, value);
        }

        public static readonly DependencyProperty StudyDateProperty =
            DependencyProperty.Register(
                nameof(StudyDate),
                typeof(DateTime?),
                typeof(StudyThumbnail),
                new PropertyMetadata(null));

        public DateTime? StudyDate
        {
            get => (DateTime?)GetValue(StudyDateProperty);
            set => SetValue(StudyDateProperty, value);
        }

        public static readonly DependencyProperty StudyDescriptionProperty =
            DependencyProperty.Register(
                nameof(StudyDescription),
                typeof(string),
                typeof(StudyThumbnail),
                new PropertyMetadata(string.Empty));

        public string StudyDescription
        {
            get => (string)GetValue(StudyDescriptionProperty);
            set => SetValue(StudyDescriptionProperty, value);
        }

        public static readonly DependencyProperty ModalityProperty =
            DependencyProperty.Register(
                nameof(Modality),
                typeof(string),
                typeof(StudyThumbnail),
                new PropertyMetadata("CR"));

        /// <summary>
        /// DICOM Modality (CR, DR, CT, MR, etc.)
        /// </summary>
        public string Modality
        {
            get => (string)GetValue(ModalityProperty);
            set => SetValue(ModalityProperty, value);
        }

        public static readonly DependencyProperty StudyStatusProperty =
            DependencyProperty.Register(
                nameof(StudyStatus),
                typeof(StudyStatus),
                typeof(StudyThumbnail),
                new PropertyMetadata(StudyStatus.Waiting));

        public StudyStatus StudyStatus
        {
            get => (StudyStatus)GetValue(StudyStatusProperty);
            set => SetValue(StudyStatusProperty, value);
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(
                nameof(IsSelected),
                typeof(bool),
                typeof(StudyThumbnail),
                new PropertyMetadata(false));

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }
    }

    /// <summary>
    /// Study workflow status.
    /// </summary>
    public enum StudyStatus
    {
        Waiting,      // 대기 중
        InProgress,   // 진행 중
        Completed,    // 완료
        Error         // 오류
    }
}
