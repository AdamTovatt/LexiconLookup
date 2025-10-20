using EasyReasy;

namespace LexiconLookup.Tests
{
    public static class TestResources
    {
        [ResourceCollection(typeof(EmbeddedResourceProvider))]
        public static class Dictionary
        {
            public static readonly Resource WordsCsv = new Resource("Resources/words.csv");
        }
    }
}
