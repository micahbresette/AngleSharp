﻿using AngleSharp;
using AngleSharp.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;

namespace UnitTests.Library
{
    [TestClass]
    public class AnalysisWindowTests
    {
        AnalysisWindow window;

        [TestInitialize]
        public void CreateDefaultAnalysisWindow()
        {
            window = new AnalysisWindow();
            window.ScreenX = 0;
            window.ScreenY = 0;
            window.OuterHeight = 768;
            window.OuterWidth = 1024;
        }

        [TestMethod]
        public void GetComputedStyleTrivialInitialScenario()
        {
            var sourceCode = "<!doctype html><head><style>p > span { color: blue; } span.bold { font-weight: bold; }</style></head><body><div><p><span class='bold'>Bold text";

            var document = DocumentBuilder.Html(sourceCode);
            Assert.IsNotNull(document);
            window.Document = document;

            var element = document.QuerySelector("span.bold");
            Assert.IsNotNull(element);

            Assert.AreEqual("span", element.TagName);
            Assert.AreEqual("bold", element.ClassName);

            var style = window.GetComputedStyle(element);
            Assert.IsNotNull(style);
            Assert.AreEqual(2, style.Length);
        }

        [TestMethod]
        public void GetComputedStyleImportantHigherNoInheritance()
        {
            var source = new StringBuilder("<!doctype html> ");

            var styles = new StringBuilder("<head><style>");
            styles.Append("p {text-align: center;}");
            styles.Append("p > span { color: blue; }");
            styles.Append("p > span { color: red; }");
            styles.Append("span.bold { font-weight: bold !important; }");
            styles.Append("span.bold { font-weight: lighter; }");

            styles.Append("#prioOne { color: black; }");
            styles.Append("div {color: green; }");
            styles.Append("</style></head>");

            var body = new StringBuilder("<body>");
            body.Append("<div><p><span class='bold'>Bold text</span></p></div>");
            body.Append("<div id='prioOne'>prioOne</div>");
            body.Append("</body>");

            source.Append(styles);
            source.Append(body);

            var document = DocumentBuilder.Html(source.ToString());
            Assert.IsNotNull(document);
            window.Document = document;

            // checks for element with text bold text
            var element = document.QuerySelector("span.bold");
            Assert.IsNotNull(element);
            Assert.AreEqual("span", element.TagName);
            Assert.AreEqual("bold", element.ClassName);

            var computedStyle = window.GetComputedStyle(element);
            Assert.AreEqual("red", computedStyle.Color);
            Assert.AreEqual("bold", computedStyle.FontWeight);
            Assert.AreEqual(3, computedStyle.Length);
        }

        [TestMethod]
        public void GetComputedStyleHigherMatchingPrio()
        {
            var source = new StringBuilder("<!doctype html> ");

            var styles = new StringBuilder("<head><style>");
            styles.Append("p {text-align: center;}");
            styles.Append("p > span { color: blue; }");
            styles.Append("p > span { color: red; }");
            styles.Append("span.bold { font-weight: bold !important; }");
            styles.Append("span.bold { font-weight: lighter; }");

            styles.Append("#prioOne { color: black; }");
            styles.Append("div {color: green; }");
            styles.Append("</style></head>");

            var body = new StringBuilder("<body>");
            body.Append("<div><p><span class='bold'>Bold text</span></p></div>");
            body.Append("<div id='prioOne'>prioOne</div>");
            body.Append("</body>");

            source.Append(styles);
            source.Append(body);

            var document = DocumentBuilder.Html(source.ToString());
            Assert.IsNotNull(document);
            window.Document = document;

            // checks for element with text prioOne
            var prioOne = document.QuerySelector("#prioOne");
            Assert.IsNotNull(prioOne);
            Assert.AreEqual("div", prioOne.TagName);
            Assert.AreEqual("prioOne", prioOne.Id);

            var computePrioOneStyle = window.GetComputedStyle(prioOne);
            Assert.AreEqual("black", computePrioOneStyle.Color);
        }

        [TestMethod]
        public void GetComputedStyleUseAndPreferInlineStyle()
        {
            var source = new StringBuilder("<!doctype html> ");

            var styles = new StringBuilder("<head><style>");
            styles.Append("p > span { color: blue; }");
            styles.Append("</style></head>");

            var body = new StringBuilder("<body>");
            body.Append("<div><p><span style='color: red'>Bold text</span></p></div>");
            body.Append("</body>");

            source.Append(styles);
            source.Append(body);

            var document = DocumentBuilder.Html(source.ToString());
            Assert.IsNotNull(document);
            window.Document = document;

            // checks for element with text bold text
            var element = document.QuerySelector("p > span");
            Assert.IsNotNull(element);
            Assert.AreEqual("span", element.TagName);

            var computedStyle = window.GetComputedStyle(element);
            Assert.AreEqual("red", computedStyle.Color);
            Assert.AreEqual(1, computedStyle.Length);
        }

        [TestMethod]
        public void GetComputedStyleComplexScenario()
        {
            var sourceCode = @"<!doctype html>
<head>
<style>
p > span { color: blue; } 
span.bold { font-weight: bold; }
</style>
<style>
p { font-size: 20px; }
em { font-style: italic !important; }
.red { margin: 5px; }
</style>
<style>
#text { font-style: normal; margin: 0; }
</style>
</head>
<body>
<div><p><span class=bold>Bold <em style='color: red' class=red id=text>text</em>";

            var document = DocumentBuilder.Html(sourceCode);
            Assert.IsNotNull(document);
            window.Document = document;

            var element = document.QuerySelector("#text");
            Assert.IsNotNull(element);

            Assert.AreEqual("em", element.TagName);
            Assert.AreEqual("red", element.ClassName);
            Assert.IsNotNull(element.GetAttribute("style"));
            Assert.AreEqual("text", element.TextContent);

            var style = window.GetComputedStyle(element);
            Assert.IsNotNull(style);
            Assert.AreEqual(5, style.Length);

            Assert.AreEqual("0", style.Margin);
            Assert.AreEqual("red", style.Color);
            Assert.AreEqual("bold", style.FontWeight);
            Assert.AreEqual("italic", style.FontStyle);
            Assert.AreEqual("20px", style.FontSize);
        }
    }
}
