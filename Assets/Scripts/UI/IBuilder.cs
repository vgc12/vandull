namespace UI
{
    public interface IBuilder<out T>
    {
        T Build();
    }
}