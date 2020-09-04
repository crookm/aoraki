namespace Aoraki.Web.Contracts
{
    public interface IJournalSettings
    {
        string DbConnection { get; set; }
        string DbName { get; set; }
        string DbPostsCollection { get; set; }
    }
}