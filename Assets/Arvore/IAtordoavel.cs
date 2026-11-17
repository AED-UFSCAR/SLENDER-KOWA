// Qualquer coisa que pode ser atordoada pelo FLASH da lanterna.
// Quando o Kowa for criado, o script dele deve implementar esta interface:
//   public class Kowa : MonoBehaviour, IAtordoavel { public void Atordoar(float s) { ... } }
// (o Kowa também precisa de um Collider2D para ser encontrado)
public interface IAtordoavel
{
    void Atordoar(float duracao);
}
