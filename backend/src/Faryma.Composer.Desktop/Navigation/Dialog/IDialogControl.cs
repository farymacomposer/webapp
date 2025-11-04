namespace Faryma.Composer.Desktop.Navigation
{
    public interface IDialogControl<T> where T : DialogVM
    {
        T ViewModel { get; }
    }
}