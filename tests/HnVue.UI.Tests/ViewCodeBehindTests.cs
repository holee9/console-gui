using System.Windows;
using FluentAssertions;
using HnVue.UI.Views;
using Xunit;

namespace HnVue.UI.Tests
{
    public class ViewCodeBehindTests
    {
        [StaFact]
        [Trait("Category", "Views")]
        [Trait("View", "PatientListView")]
        public void PatientListView_DefaultConstructor_InitializesComponent()
        {
            StaRunner.Run(() =>
            {
                var view = new PatientListView();
                view.Should().NotBeNull();
                view.DataContext.Should().BeNull();
            });
        }

        [StaFact]
        [Trait("Category", "Views")]
        [Trait("View", "LoginView")]
        public void LoginView_DefaultConstructor_InitializesComponent()
        {
            StaRunner.Run(() =>
            {
                var view = new LoginView();
                view.Should().NotBeNull();
                view.DataContext.Should().BeNull();
            });
        }

        [StaFact]
        [Trait("Category", "Views")]
        [Trait("View", "QuickPinLockView")]
        public void QuickPinLockView_DefaultConstructor_InitializesComponent()
        {
            StaRunner.Run(() =>
            {
                var view = new QuickPinLockView();
                view.Should().NotBeNull();
                view.DataContext.Should().BeNull();
            });
        }

        [StaFact]
        [Trait("Category", "Views")]
        [Trait("View", "MergeView")]
        public void MergeView_DefaultConstructor_InitializesComponent()
        {
            StaRunner.Run(() =>
            {
                var view = new MergeView();
                view.Should().NotBeNull();
                view.DataContext.Should().BeNull();
            });
        }

        [StaFact]
        [Trait("Category", "Views")]
        [Trait("View", "SettingsView")]
        public void SettingsView_DefaultConstructor_InitializesComponent()
        {
            StaRunner.Run(() =>
            {
                var view = new SettingsView();
                view.Should().NotBeNull();
                view.DataContext.Should().BeNull();
            });
        }

        [StaFact]
        [Trait("Category", "Views")]
        [Trait("View", "StudylistView")]
        public void StudylistView_DefaultConstructor_InitializesComponent()
        {
            StaRunner.Run(() =>
            {
                var view = new StudylistView();
                view.Should().NotBeNull();
                view.DataContext.Should().BeNull();
            });
        }

        [StaFact]
        [Trait("Category", "Views")]
        [Trait("View", "SystemAdminView")]
        public void SystemAdminView_DefaultConstructor_InitializesComponent()
        {
            StaRunner.Run(() =>
            {
                var view = new SystemAdminView();
                view.Should().NotBeNull();
                view.DataContext.Should().BeNull();
            });
        }

        [StaFact]
        [Trait("Category", "Views")]
        [Trait("View", "DoseDisplayView")]
        public void DoseDisplayView_DefaultConstructor_InitializesComponent()
        {
            StaRunner.Run(() =>
            {
                var view = new DoseDisplayView();
                view.Should().NotBeNull();
                view.DataContext.Should().BeNull();
            });
        }

        [StaFact]
        [Trait("Category", "Views")]
        [Trait("View", "ImageViewerView")]
        public void ImageViewerView_DefaultConstructor_InitializesComponent()
        {
            StaRunner.Run(() =>
            {
                var view = new ImageViewerView();
                view.Should().NotBeNull();
                view.DataContext.Should().BeNull();
            });
        }

        [StaFact]
        [Trait("Category", "Views")]
        [Trait("View", "CDBurnView")]
        public void CDBurnView_DefaultConstructor_InitializesComponent()
        {
            StaRunner.Run(() =>
            {
                var view = new CDBurnView();
                view.Should().NotBeNull();
                view.DataContext.Should().BeNull();
            });
        }

        [StaFact]
        [Trait("Category", "Views")]
        [Trait("View", "AddPatientProcedureView")]
        public void AddPatientProcedureView_DefaultConstructor_InitializesComponent()
        {
            StaRunner.Run(() =>
            {
                var view = new AddPatientProcedureView();
                view.Should().NotBeNull();
                view.DataContext.Should().BeNull();
            });
        }
    }
}
