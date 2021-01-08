namespace Trakx.Utils.Testing.Interfaces
{
    public interface ISecretsProvider<T>
    {
        T GetSecrets();
    }
}