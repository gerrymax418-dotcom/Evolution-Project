namespace VerasStudios.Hopper
{
    // HOP Start of example
    public class Example
    {
        private string quoteExample = "// HOP this hop will not be shown because it's inside of quotes";

        //toDo remove this method
        void Start()
        {
            System.Diagnostics.Debug.WriteLine(quoteExample); //   ToDO rewrite this line
        }

        // Bug this method isn't working for some reason
        private bool returnTrueAlways()
        {
            return false;
        }
    }
    //hop end of example
}