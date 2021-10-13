using ComX.Common.ApiClients.Builders;
using ComX.Common.ApiClients.Tests.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;

namespace ComX.Common.ApiClients.Tests
{
    public sealed class RequestBuilderTests
    {
        [Test]
        public void RequestBuilderTest1()
        {
            WebRequestUriBuilder requestBuilder = new WebRequestUriBuilder();
            requestBuilder.SetPath("a", "/b/c/");
            Uri uri = requestBuilder.BuildUri();
            string relative = uri.ToString();

            Assert.AreEqual("a/b/c", relative);
        }

        [Test]
        public void RequestBuilderTest2()
        {
            WebRequestUriBuilder requestBuilder = new WebRequestUriBuilder();
            requestBuilder.SetPath("a", "/b/c/");
            requestBuilder.SetFragment("fragment");
            Uri uri = requestBuilder.BuildUri();

            string relative = uri.ToString();

            Assert.AreEqual("a/b/c#fragment", relative);
        }

        [Test]
        public void RequestBuilderTest3()
        {
            WebRequestUriBuilder requestBuilder = new WebRequestUriBuilder();
            requestBuilder.SetPath("a", "/b/c/");
            requestBuilder.SetFragment("##fragment");
            Uri uri = requestBuilder.BuildUri();

            string relative = uri.ToString();

            Assert.AreEqual("a/b/c##fragment", relative);
        }

        [Test]
        public void RequestBuilderTest4()
        {
            WebRequestUriBuilder requestBuilder = new WebRequestUriBuilder();
            requestBuilder.SetPath("a", "/b/c/");
            requestBuilder.AddQueryParameter("name", "jon");
            requestBuilder.SetFragment("#fragment");
            Uri uri = requestBuilder.BuildUri();

            string relative = uri.ToString();

            Assert.AreEqual("a/b/c?name=jon#fragment", relative);
        }

        [Test]
        public void RequestBuilderTest5()
        {
            WebRequestUriBuilder requestBuilder = new WebRequestUriBuilder();
            requestBuilder.SetPath("a", "/b/c/");
            requestBuilder.AddQueryParameter("name", "jon");
            requestBuilder.AddQueryParameter("surname", "don");
            requestBuilder.SetFragment("#fragment");
            Uri uri = requestBuilder.BuildUri();

            string relative = uri.ToString();

            Assert.AreEqual("a/b/c?name=jon&surname=don#fragment", relative);
        }

        [Test]
        public void RequestBuilderTest6()
        {
            WebRequestUriBuilder requestBuilder = new WebRequestUriBuilder();
            requestBuilder.AddQueryParameter("name", "jon");
            Uri uri = requestBuilder.BuildUri();

            string relative = uri.ToString();

            //KEEP THIS DESIGN. THE BASE URI COMES WITH THE HTTP CLIENT
            //AND IF IT IS www.localhost.com, we only want to add the ?name=jon
            Assert.AreEqual("?name=jon", relative);
        }

        [Test]
        public void RequestUriBuilder_Guid()
        {
            Guid guid = Guid.Parse("ea1c00be-18ca-4379-ab06-fcb006b4d325");
            WebRequestUriBuilder requestBuilder = new WebRequestUriBuilder();
            requestBuilder.AddQueryParameter("name", guid);
            Uri uri = requestBuilder.BuildUri();

            string relative = uri.ToString();

            //KEEP THIS DESIGN. THE BASE URI COMES WITH THE HTTP CLIENT
            //AND IF IT IS www.localhost.com, we only want to add the ?name=jon
            Assert.AreEqual("?name=ea1c00be-18ca-4379-ab06-fcb006b4d325", relative);
        }

        [Test]
        public void RequestUriBuilder_Bool()
        {
            WebRequestUriBuilder requestBuilder = new WebRequestUriBuilder();
            requestBuilder.AddQueryParameter("name", true);
            Uri uri = requestBuilder.BuildUri();

            string relative = uri.ToString();

            //KEEP THIS DESIGN. THE BASE URI COMES WITH THE HTTP CLIENT
            //AND IF IT IS www.localhost.com, we only want to add the ?name=jon
            Assert.AreEqual("?name=true", relative);
        }

        [Test]
        public void RequestUriBuilder_double()
        {
            WebRequestUriBuilder requestBuilder = new WebRequestUriBuilder();
            requestBuilder.AddQueryParameter("name", 3.14);
            Uri uri = requestBuilder.BuildUri();

            string relative = uri.ToString();

            //KEEP THIS DESIGN. THE BASE URI COMES WITH THE HTTP CLIENT
            //AND IF IT IS www.localhost.com, we only want to add the ?name=jon
            Assert.AreEqual("?name=3.14", relative);
        }

        [Test]
        public void RequestUriBuilder_EnumWithSimpleMethod()
        {
            WebRequestUriBuilder requestBuilder = new WebRequestUriBuilder();
            requestBuilder.AddQueryParameter("name", Colors.Red);
            Uri uri = requestBuilder.BuildUri();

            string relative = uri.ToString();

            //KEEP THIS DESIGN. THE BASE URI COMES WITH THE HTTP CLIENT
            //AND IF IT IS www.localhost.com, we only want to add the ?name=jon
            Assert.AreEqual("?name=1", relative);
        }

        [Test]
        public void RequestUribuilder_ComplexObject()
        {
            var value = new Person { Name = "John" };
            WebRequestUriBuilder requestBuilder = new WebRequestUriBuilder();
            requestBuilder.AddQueryParameter("name", value);
            Uri uri = requestBuilder.BuildUri();

            string relative = uri.ToString();

            //KEEP THIS DESIGN. THE BASE URI COMES WITH THE HTTP CLIENT
            //AND IF IT IS www.localhost.com, we only want to add the ?name=jon
            Assert.AreEqual("?name=%257b%2522Name%2522%253a%2522John%2522%252c%2522Color%2522%253a0%257d", relative);
        }

        [Test]
        public void RequestUribuilder_ModelProperty_Attribute()
        {
            var value = new Person { GivenName = "John" };
            WebRequestUriBuilder requestBuilder = new WebRequestUriBuilder();
            requestBuilder.AddQueryParameter<Person, string, JsonPropertyAttribute>(x => x.GivenName, value.GivenName, attr =>
            {
                return attr.PropertyName;
            });

            Uri uri = requestBuilder.BuildUri();

            string relative = uri.ToString();

            //KEEP THIS DESIGN. THE BASE URI COMES WITH THE HTTP CLIENT
            //AND IF IT IS www.localhost.com, we only want to add the ?name=jon
            Assert.AreEqual("?NumeleDat=John", relative);
        }

        [Test]
        public void RequestUriBuilder_Enum_UseDisplayMemberAttribute()
        {
            WebRequestUriBuilder requestBuilder = new WebRequestUriBuilder();

            requestBuilder.AddEnumQueryParameter<Colors, DisplayAttribute>("culoare", Colors.Red, attr =>
            {
                return attr.Name;
            });

            Uri uri = requestBuilder.BuildUri();

            string relative = uri.ToString();

            //KEEP THIS DESIGN. THE BASE URI COMES WITH THE HTTP CLIENT
            //AND IF IT IS www.localhost.com, we only want to add the ?name=jon
            Assert.AreEqual("?culoare=Rosu", relative);
        }

        [Test]
        public void RequestUriBuilder_PropertyAtrr_EnumFieldAttr()
        {
            Person person = new Person { Color = Colors.Red };

            WebRequestUriBuilder requestBuilder = new WebRequestUriBuilder();

            requestBuilder.AddEnumQueryParameter<Person, Colors, JsonPropertyNameAttribute, DisplayAttribute>
                (property: x => x.Color, 
                 value: Colors.Red,
                 getKey: key => key.Name, 
                 getValue: value => value.Name);

            Uri uri = requestBuilder.BuildUri();

            string relative = uri.ToString();

            //KEEP THIS DESIGN. THE BASE URI COMES WITH THE HTTP CLIENT
            //AND IF IT IS www.localhost.com, we only want to add the ?name=jon
            Assert.AreEqual("?CuloareaMea=Rosu", relative);
        }

        [Test]
        public void RequestBuilderTest7()
        {
            WebRequestUriBuilder requestBuilder = new WebRequestUriBuilder();
            Uri uri = requestBuilder.BuildUri();

            string relative = uri.ToString();

            //KEEP DESIGN. When the HttpRequestMessage is created and the relative uri is passed, if it's string.Emtpty
            //then the requesturi from the mesage will be set to null internally.
            Assert.AreEqual("", relative);
        }

        //[Test]
        public void Test()
        {
            WebRequestUriBuilder requestBuilder = new WebRequestUriBuilder();
            requestBuilder.AddQueryParameter("name", "Jon");
            requestBuilder.AddQueryParameter("surname", "Corona");
            requestBuilder.SetFragment("###fragment");
            var uri = requestBuilder.BuildUri();
        }
    }
}
