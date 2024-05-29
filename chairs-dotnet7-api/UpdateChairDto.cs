namespace chairs_dotnet7_api
{
    public class UpdateChairDto
    {
        public string Nombre { get; set; }  = string.Empty;
        public string Tipo { get; set; }  = string.Empty;
        public string Material { get; set; }  = string.Empty;
        public string Color { get; set; }  = string.Empty;
        public int Altura { get; set; }  
        public int Anchura { get; set; } 
        public int Profundidad { get; set; }  
        public int Precio { get; set; } 
    }
}