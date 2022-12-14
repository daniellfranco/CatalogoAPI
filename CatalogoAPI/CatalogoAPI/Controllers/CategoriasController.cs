using AutoMapper;
using CatalogoAPI.DTOs;
using CatalogoAPI.Repository;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace CatalogoAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CategoriasController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public CategoriasController(IUnitOfWork context, ILogger<CategoriasController> logger, IMapper mapper)
        {
            _uow = context;
            _logger = logger;
            _mapper = mapper;
        }


        [HttpGet]
        public ActionResult<IEnumerable<CategoriaDTO>> GetCategorias()
        {
            //throw new Exception();

            // isso será logado no console
            _logger.LogInformation("========== GET api/categorias =============");

            // Take limita a quantidade de resultados para não sobrecarregar o sistema.
            // esse get é o get do repositório
            var categorias = _uow.CategoriaRepository.Get().Take(10).ToList();
            if (categorias is null)
                return NotFound("Categorias não encontradas...");
            var categoriasDto = _mapper.Map<List<CategoriaDTO>>(categorias);

            return Ok(categoriasDto);
        }

        [HttpGet("produtos")]
        public ActionResult<IEnumerable<CategoriaDTO>> GetCategoriasProdutos()
        {
            // Recomendado nunca retornar objetos relacionados sem um filtro,
            // aqui está usando Where para retornar somente os id menor ou igual a 10.
            var categorias = _uow.CategoriaRepository.GetCategoriasProdutos()
                .Where(c => c.CategoriaId <= 10).ToList();
            if (categorias is null)
                return NotFound("Categorias não encontradas...");

            var categoriasDto = _mapper.Map<List<CategoriaDTO>>(categorias);

            return Ok(categoriasDto);
        }

        // Define que vai receber um id, e restringe a ser um inteiro.
        [HttpGet("{id:int}", Name = "ObterCategoria")]
        public ActionResult<CategoriaDTO> GetCategoriasId(int id)
        {

            var categoria = _uow.CategoriaRepository.GetById(c => c.CategoriaId == id);

            _logger.LogInformation($"========== GET api/categorias/id = {id} =============");

            if (categoria is null)
            {
                _logger.LogInformation($"========== GET api/categorias/id = {id} NOT FOUND =============");
                return NotFound($"Categoria com id= {id} não localizada...");
            }

            var categoriaDto = _mapper.Map<CategoriaDTO>(categoria);
            return Ok(categoriaDto);
        }

        [HttpPost]
        public ActionResult PostCategoria(CategoriaDTO categoriaDto)
        {
            if (categoriaDto is null)
                return BadRequest();

            var categoria = _mapper.Map<Categoria>(categoriaDto);

            _uow.CategoriaRepository.Add(categoria);
            _uow.Commit();

            // Similar ao CreatedAtAction mas informa uma rota para o nome
            // definido na action get ao invés do nome da action,
            // necessário aqui já que não estamos dando nomes especificos
            // para as actions
            return new CreatedAtRouteResult("ObterCategoria",
                new { id = categoriaDto.CategoriaId }, categoriaDto);
        }


        // Put = Atualização COMPLETA do categoria (não permite parcial)
        [HttpPut("{id:int}")]
        public ActionResult PutCategoria(int id, CategoriaDTO categoriaDto)
        {
            // Quando enviar os dados do categoria tem que
            // informar o id do categoria também.
            // Então torna-se necessario conferir os id.
            if (id != categoriaDto.CategoriaId)
            {
                return BadRequest($"O id informado ({id}) não é o mesmo id recebido para atualização ({categoriaDto.CategoriaId})");
            }

            // Confere se essa categoria existe mesmo na DB
            var categoria = _uow.CategoriaRepository.GetById(p => p.CategoriaId == id);
            if (categoria is null)
            {
                return NotFound($"Categoria com id= {id} não localizada...");
            }

            categoria = _mapper.Map<Categoria>(categoriaDto);

            _uow.CategoriaRepository.Update(categoria);
            _uow.Commit();

            return Ok(categoriaDto);
        }

        [HttpDelete("{id:int}")]
        public ActionResult DeleteCategoria(int id)
        {
            var categoria = _uow.CategoriaRepository.GetById(c => c.CategoriaId == id);

            if (categoria is null)
                return NotFound($"Categoria com id= {id} não localizada...");

            _uow.CategoriaRepository.Delete(categoria);
            _uow.Commit();

            var categoriaDto = _mapper.Map<Categoria>(categoria);

            return Ok(categoriaDto);
        }

        /*

        // Detalhes de Async/Task/Await na classe ProdutosController.

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Categoria>>> GetCategoriasAsync()
        {
            //throw new Exception();

            // isso será logado no console
            _logger.LogInformation("========== GET api/categorias =============");

            // AsNoTracking melhora a performance mas só deve ser usado em Gets
            // Take limita a quantidade de resultados para não sobrecarregar o sistema.
            var categorias = await _context.Categorias.AsNoTracking().Take(10).ToListAsync();
            if (categorias is null)
                return NotFound("Categorias não encontradas...");

            return Ok(categorias);
        }

        [HttpGet("produtos")]
        public async Task<ActionResult<IEnumerable<Categoria>>> GetCategoriasProdutosAsync()
        {
            // Recomendado nunca retornar objetos relacionados sem um filtro,
            // aqui está usando Where para retornar somente os id menor ou igual a 10.
            var categorias = await _context.Categorias.Include(p => p.Produtos)
                .AsNoTracking().Where(c => c.CategoriaId <= 10).ToListAsync();
            if (categorias is null)
                return NotFound("Categorias não encontradas...");

            return Ok(categorias);
        }

        // Define que vai receber um id, e restringe a ser um inteiro.
        [HttpGet("{id:int}", Name = "ObterCategoria")]
        public async Task<ActionResult<Categoria>> GetCategoriasIdAsync(int id)
        {
            // First busca e retorna o primeiro resultado compativel, senao ele retorna uma excessão.
            // FirstOrDefault retorna o primeiro resultado compativel, senao ele retorna um null.
            var categoria = await _context.Categorias.AsNoTracking().FirstOrDefaultAsync(c => c.CategoriaId == id);

            _logger.LogInformation($"========== GET api/categorias/id = {id} =============");

            if (categoria is null)
            {
                _logger.LogInformation($"========== GET api/categorias/id = {id} NOT FOUND =============");
                return NotFound($"Categoria com id= {id} não localizada...");
            }
            return Ok(categoria);

        }

        [HttpPost]
        public async Task<ActionResult> PostCategoriaAsync(Categoria categoria)
        {
            if (categoria is null)
                return BadRequest();

            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            // Similar ao CreatedAtAction mas informa uma rota para o nome
            // definido na action get ao invés do nome da action,
            // necessário aqui já que não estamos dando nomes especificos
            // para as actions
            return new CreatedAtRouteResult("ObterCategoria",
                new { id = categoria.CategoriaId }, categoria);
        }


        // Put = Atualização COMPLETA do categoria (não permite parcial)
        [HttpPut("{id:int}")]
        public async Task<ActionResult> PutCategoriaAsync(int id, Categoria categoria)
        {
            // Quando enviar os dados do categoria tem que
            // informar o id do categoria também.
            // Então torna-se necessario conferir os id.
            if (id != categoria.CategoriaId)
            {
                return BadRequest($"O id informado ({id}) não é o mesmo id recebido para atualização ({categoria.CategoriaId})");
            }

            // Como estamos trabalhando em um cenario "desconectado"
            // (os dados estão dentro da variavel _context)
            // o contexto precisa ser informado que categoria está em um
            // estado modificado. Para isso usamos o metodo Entry do contexto.
            _context.Entry(categoria).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(categoria);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteCategoriaAsync(int id)
        {
            var categoria = await _context.Categorias.FirstOrDefaultAsync(c => c.CategoriaId == id);

            if (categoria is null)
                return NotFound($"Categoria com id= {id} não localizada...");

            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();

            return Ok(categoria);
        }

        */
    }
}
