# xml-transform
A simple wrapper around the MSBuild TransformXml task from Visual Studio 2015. 

Keep in mind that the XmlTransformer works on the string contents and not the file paths of the transform files.

## Install

#### Package Manager

Search for **XmlTransform.VS2015** in Nuget.

#### Package Manager Console

Type the following into the console:

    Install-Package XmlTransform.VS2015

## Contribute

Pull requests are gladly accepted.

## [How to](#how-to)

The following code examples show you how to use XmlTransform.

### Apply a single transform.

```csharp
using XmlTransform;

namespace XmlTransformTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var transformer = new XmlTransformer();
            var source = GetSourceConfigFileAsString();
            var transform = GetTransformConfigFileAsString();
            var result = string.Empty;
            try {
                result = transformer.ApplyTransform(source, transform);
            } catch(XmlTransformerException e){
                result = e.Message;
            }
            Console.WriteLine(result);
        }
    }
}
```


### Apply multiple transforms.

```csharp
using XmlTransform;

namespace XmlTransformTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var transformer = new XmlTransformer();
            var source = GetSourceConfigFileAsString();
            var transforms = GetTransformConfigFilesAsStringArray();
            var result = string.Empty;
            try {
                result = transformer.ApplyTransforms(source, transforms);
            } catch(XmlTransformerException e){
                result = e.Message;
            }
            Console.WriteLine(result);
        }
    }
}
```


