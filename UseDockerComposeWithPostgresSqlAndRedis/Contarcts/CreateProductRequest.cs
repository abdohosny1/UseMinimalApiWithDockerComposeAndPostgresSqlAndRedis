namespace UseDockerComposeWithPostgresSqlAndRedis.Contarcts
{
    public class CreateProductRequest
    {
        public string Name { get; set; }
        public int Price { get; set; }
    }

    public class UpdateProduct
    {
        public string Name { get; set; }
        public int Price { get; set; }
    }
}
