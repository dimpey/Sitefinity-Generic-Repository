namespace Impey.Sitefinity.Repository.Fields
{
    public class Image
    {
        public string Url { get; private set; }
        public string Alt { get; private set; }
        public string Title { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public Image(Telerik.Sitefinity.Libraries.Model.Image image)
        {
            Url = image.Url;
            Alt = image.AlternativeText;
            Title = image.Title;
            Width = image.Width;
            Height = image.Height;
        }

        public static implicit operator Image(Telerik.Sitefinity.Libraries.Model.Image image)
        {
            return new Image(image);
        }
    }
}
