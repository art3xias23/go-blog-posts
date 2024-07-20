namespace blog_2_charp_urlfor;

public interface ITemplateParser
{
    string Render(object model, string filepath);
    string RenderFile(object model, string filepath);
}