using System;
using Xunit;
using XmlTransform.Internals;
using Xunit.Sdk;

namespace XmlTransform.Tests
{
    public class XmlTransformerTests
    {
        #region Test Congfig Data
        private const string TestSource = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<configuration>
  <appSettings>
    <add key=""ServiceDisplayName"" value=""#SOURCE_SERVICE_DISPLAY_NAME#"" />
    <add key=""RefreshRate"" value=""#SOURCE_REFRESH_RATE#"" />
    <add key=""Threshold"" value=""#SOURCE_THRESHOLD#""/>
  </appSettings>
  <connectionStrings>
    <add name=""DefaultDB"" connectionString=""#SOURCE_CONNECTION_STRING#"" />
  </connectionStrings>
  <system.web>
    <httpRuntime targetFramework=""4.5"" />
    <customErrors mode=""RemoteOnly"" defaultRedirect=""~/error.aspx"" redirectMode=""ResponseRewrite"" />
    <pages>
      <namespaces>
        <add namespace=""System.Web.Helpers"" />
        <add namespace=""System.Web.Mvc"" />
        <add namespace=""System.Web.Mvc.Ajax"" />
        <add namespace=""System.Web.Mvc.Html"" />
        <add namespace=""System.Web.Routing"" />
        <add namespace=""System.Web.WebPages"" />
      </namespaces>
    </pages>
  </system.web>
  <system.webServer>
    <security>
      <authentication>
        <windowsAuthentication enabled=""true"" />
      </authentication>
    </security>
  </system.webServer>
</configuration>";

        private const string TestTransformOne = @"<?xml version=""1.0"" ?>
<configuration xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"" >
  <connectionStrings>
    <add name=""DefaultDB"" connectionString=""#TRANSFORM_ONE_CONNECTIONG_STRING#"" xdt:Transform=""SetAttributes"" xdt:Locator=""Match(name)"" />
  </connectionStrings>
  <appSettings>
    <add key=""ServiceDisplayName"" value=""#TRANSFORM_ONE_SERVICE_DISPLAY_NAME#"" xdt:Transform=""SetAttributes"" xdt:Locator=""Match(key)"" />
    <add key=""RefreshRate"" value=""#TRANSFORM_ONE_REFRESH_RATE#"" xdt:Transform=""SetAttributes"" xdt:Locator=""Match(key)"" />
  </appSettings>
  <system.web>
    <compilation debug=""#TRANSFORM_ONE_DEBUG_VALUE#"" targetFramework=""4.5"" xdt:Transform=""InsertIfMissing"" />
  </system.web>
</configuration>";

        private const string TestTransformTwo = @"<?xml version=""1.0"" ?>
<configuration xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"" >
  <connectionStrings>
    <add name=""DefaultDB"" connectionString=""Data Source=#TRANSFORM_TWO_CONNECTION_STRING#"" xdt:Transform=""SetAttributes"" xdt:Locator=""Match(name)"" />
  </connectionStrings>
  <appSettings>
    <add key=""ServiceDisplayName"" value=""#TRANSFORM_TWO_SERVICE_DISPLAY_NAME#"" xdt:Transform=""SetAttributes"" xdt:Locator=""Match(key)"" />
    <add key=""RefreshRate"" value=""#TRANSFORM_TWO_REFRESH_RATE#"" xdt:Transform=""SetAttributes"" xdt:Locator=""Match(key)"" />
  </appSettings>
  <system.web>
    <compilation debug=""#TRANSFORM_TWO_DEBUG_VALUE#"" targetFramework=""4.5"" xdt:Transform=""InsertIfMissing"" />
  </system.web>
</configuration>";


        private const string TestMalformedConfig = @"<?xml version=""1.0"" ?>
<config xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"" >
    <compilation debug=""#TRANSFORM_TWO_DEBUG_VALUE#"" targetFramework=""4.5"" xdt:Transform=""InsertIfMissing"" />
  </system.web>
</config>";
        #endregion

        [Theory]
        [InlineData("", TestTransformOne, "Source cannot be null, empty or consist of whitespace only.")]
        [InlineData(TestSource, null, "Transforms cannot consist of only null, empty or whitespace elements.")]
        [InlineData(TestSource, "", "Transforms cannot consist of only null, empty or whitespace elements.")]
        public void ApplyTransform_NullOrEmptyInputs_ThrowsXmlTransformerException(string source, string transform, string expectedErrorMessage)
        {
            var transformer = new XmlTransformer();
            var expected = expectedErrorMessage;

            var exception = Assert.Throws<XmlTransformerException>(() => transformer.ApplyTransform(source, transform));

            Assert.Equal(expected, exception.Message);
        }


        [Fact]
        public void ApplyTransform_MalformedSource_ThrowsXmlTransformerException()
        {
            var transformer = new XmlTransformer();
            var source = TestMalformedConfig;
            var transform = TestTransformOne;

            var exception = Assert.Throws<XmlTransformerException>(() => transformer.ApplyTransform(source, transform));
            
            var actual = exception.Message.StartsWith(TransformerErrors.FailedToLoadSource);


            Assert.Equal(true, actual);
        }

        [Fact]
        public void ApplyTransform_MalformedTransform_ThrowsXmlTransformerException()
        {
            var transformer = new XmlTransformer();
            var source = TestSource;
            var transform = TestMalformedConfig;

            var exception = Assert.Throws<XmlTransformerException>(() => transformer.ApplyTransform(source, transform));
            
            var actual = exception.Message.StartsWith(TransformerErrors.ErrorWhileApplyingTransformFrom);


            Assert.Equal(true, actual);
        }

        [Fact]
        public void ApplyTransforms_SecondMalformedTransform_ThrowsXmlTransformerException()
        {
            var transformer = new XmlTransformer();
            var source = TestSource;
            var transforms = new[]
            {
                TestTransformOne,
                TestMalformedConfig
            };

            var exception = Assert.Throws<XmlTransformerException>(() => transformer.ApplyTransforms(source, transforms));

            var actual = exception.Message.StartsWith(TransformerErrors.ErrorWhileApplyingTransformFrom + "2.");

            Assert.Equal(true, actual);
        }

        [Fact]
        public void ApplyTransforms_TransformDoesntChangeSource_ReturnsSource()
        {
            var transformer = new XmlTransformer();
            var source = "<configuration></configuration>";
            var transform = "<configuration></configuration>";
            var expected = "<configuration></configuration>";

            var actual = transformer.ApplyTransform(source, transform);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ApplyTransforms_SameKeysUpdatedByTwoTransforms_ReturnsLastAppliedTransformUpdate()
        {
            var transformer = new XmlTransformer();
            var source = TestSource;
            var transforms = new[] { TestTransformOne, TestTransformTwo };

            var result = transformer.ApplyTransforms(source, transforms);

            Assert.True(result.Contains("#TRANSFORM_TWO_CONNECTION_STRING#"));
            Assert.True(result.Contains("#TRANSFORM_TWO_SERVICE_DISPLAY_NAME#"));
        }

    }
}
