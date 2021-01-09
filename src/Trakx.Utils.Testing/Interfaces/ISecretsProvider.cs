namespace Trakx.Utils.Testing.Interfaces
{
    public interface ISecretsProvider<out T>
    {
        T GetSecrets();
    }
}