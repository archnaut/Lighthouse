using System;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using Lighthouse.Client.Logging;
using Lighthouse.Silverlight4.SampleXapWithTests.TestsForBugs.PagesForBugs;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lighthouse.Silverlight4.SampleXapWithTests.TestsForBugs
{
    [TestClass]
    public class TestsForLighthouseV1 : SilverlightTest
    {
        [TestMethod]
        [Asynchronous]
        [Timeout(30000)]
        public void AsyncTesting_WhenControlIsDisplayed_HaveSameProperty_Item10521()
        {
            var textBoxInsteadOfPicker = new TextBox() { Text = "some text" };

            textBoxInsteadOfPicker.Loaded += (sender, args) =>
                                 {
                                     Assert.AreEqual("some text", textBoxInsteadOfPicker.Text);
                                     EnqueueTestComplete();
                                 };

            this.TestPanel.Children.Add(textBoxInsteadOfPicker);
        }

        [TestMethod]
        public void ClientLoggingTesting_WhenClientSendsLogMessage_ShouldNotThrowException()
        {
            var testResultsInformer = HtmlPage.Window.Eval("window.external") as ScriptObject;
            if (testResultsInformer != null)
            {
                try
                {
                    testResultsInformer.Invoke("ClientLogMessage", "sent from client");
                    Assert.IsTrue(true);
                }
                catch (Exception e)
                {
                    Assert.IsTrue(false, string.Format("Exception while trying to send ClientLogMessage: {0}", e.Message));
                }
            }
            else
            {
                Assert.IsTrue(false, "Cannot send ClientLogMessage. Cannot hook to the outside world");
            }
        }

        [TestMethod]
        public void ClientLoggingTesting_WhenClientSendsLogMessageViaClientLoggerClass_ShouldSucceed()
        {
            IClientLogger logger = new ClientLogger();
            var result = logger.SendClientLogMessage("hello world");
            Assert.IsTrue(result);
        }


        [TestMethod]
        [Asynchronous]
        [Timeout(10000)]
        public void TestMethodwithScrollWiewer()
        {
            string text = "Some text to display";
            //var mock = new Mock<IViewModel>();
            //mock.SetupGet(vm => vm.TheText).Returns(text);
            var cl = new ClientLogger();

            var view = new TextBlockInScrollViewerBugPage() {DataContext = text};
            cl.SendClientLogMessage("starting wtihout scrollviewer");   
            view.Loaded += (s, e) =>
            {
                cl.SendClientLogMessage("loaded view");
                var textBlock = view.FindName("TheName") as TextBlock;
                Assert.AreEqual(text, textBlock.Text);
                EnqueueTestComplete();
            };

            this.TestPanel.Children.Add(view);
        }

        [TestMethod]
        [Asynchronous]
        [Timeout(10000)]
        public void TestMethodWithoutScrollviewer()
        {
            string text = "Some text to display";
            //var mock = new Mock<IViewModel>();
            //mock.SetupGet(vm => vm.TheText).Returns(text);
            var cl = new ClientLogger();

            cl.SendClientLogMessage("starting wtihout scrollviewer");
            var view = new TextBoxWithoutScrollviewerBugPage() { DataContext = text };
            view.Loaded += (s, e) =>
                               {
                                    cl.SendClientLogMessage("loaded view");
                                    var textBlock = view.FindName("TheName") as TextBlock;
                                    Assert.AreEqual(text, textBlock.Text);
                                    EnqueueTestComplete();
            };

            this.TestPanel.Children.Add(view);
        }

        [TestMethod]
        [Asynchronous]
        [Timeout(3000)]
        public void TestMethodWithScrollViewerOriginal()
        {
            string text = "Some text to display";
            var cl = new ClientLogger();

            cl.SendClientLogMessage("starting original with scrollviewer");
            var view = new TextBlockInScrollViewerBugPage() { DataContext = text };
            var textBlock = view.FindName("TheName") as TextBlock;
            textBlock.Loaded += (s, e) =>
            {
                cl.SendClientLogMessage("textbox loaded");
                Assert.AreEqual(text, ((TextBlock)s).Text);
                EnqueueTestComplete();
            };

            this.TestPanel.Children.Add(view);
        }



        
    }
}