using System.Windows;
using System.Windows.Controls;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lighthouse.Silverlight4.SampleTests
{
    [TestClass]
    public class TestClass1Tests : SilverlightTest
    {

        [TestMethod]
        public void FirstTest_ShouldPass()
        {
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void SecondTest_ShouldFail()
        {
            Assert.IsTrue(false);
        }

        [TestMethod]
        public void ThirdTest_ShouldBeFilteredNoLetter_a()
        {
            Assert.IsTrue(false);
        }

        [TestMethod]
        [Tag("DateRangePicker")]
        [Asynchronous]
        [Timeout(30000)]
        public void WhenDisplayedShouldSetDatePickerRangeToOneMonth()
        {
            var textBox = new TextBox() { Text="some text" };
            this.TestPanel.Children.Add(textBox);
            bool loaded = false;
            textBox.Loaded += (object sender, RoutedEventArgs e) => { loaded = true; };
            EnqueueConditional(() => loaded);

            EnqueueCallback(() => Assert.AreEqual("some text", textBox.Text));

            EnqueueTestComplete();
        }


    }
}
