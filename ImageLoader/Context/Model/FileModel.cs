namespace ImageLoader.Context.Model;

public class FileModel
{
    public int Id { get; set; }
    
    public string? Key { get; set; }
    
    public string? Path { get; set; }
    
    public string? Extension { get; set; }
    
    public DateTime Created { get; set; }
    public DateTime Expires { get; set; }
}