namespace LAB05.Tools.Navigation
{
    internal enum ViewType
    {
        DataView
        
    }

    interface INavigationModel
    {
        void Navigate(ViewType viewType);
    }
}