namespace wallspace
{
    public class Images
    {
        public int Id { get; set; }

        public string file_name { get; set; }


        //Don't ever add a function in a table template class, It will make an error if you call it without telling you why
       // public void setName(string name)//to be honset I just wanted to test if this would work
       // { this.file_name = name; }
    }
}
