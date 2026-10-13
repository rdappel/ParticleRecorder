using System.Windows.Forms.Design;

namespace ParticleRecorder.Effects;

/// <summary>PropertyGrid "..." editor that picks an image file the native preview can load.</summary>
public sealed class ImageFileNameEditor : FileNameEditor
{
    /// <summary>Image formats supported by Allegro's image addon.</summary>
    public const string ImageFilter = "Images (*.png;*.jpg;*.jpeg;*.bmp;*.tga)|*.png;*.jpg;*.jpeg;*.bmp;*.tga|All files (*.*)|*.*";

    protected override void InitializeDialog(OpenFileDialog openFileDialog)
    {
        base.InitializeDialog(openFileDialog);
        openFileDialog.Title = "Choose Particle Texture";
        openFileDialog.Filter = ImageFilter;
    }
}
