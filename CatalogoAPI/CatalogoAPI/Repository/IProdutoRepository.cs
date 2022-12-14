using Models;

namespace CatalogoAPI.Repository
{
    // Interface especifica para implementar os métodos especificos
    // ou se não houver os metodos especificos, precisa para deixar 
    // definido o tipo que neste caso é <Produto>
    public interface IProdutoRepository : IRepository<Produto>
    {
        IEnumerable<Produto> GetProdutosPorPreco();
    }
}
