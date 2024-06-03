namespace SignalRHubs.Models
{
    public class Partida
    {
        public string NombrePartida { get; set; }
        public string NombreUsuario1 { get; set; }
        public string NombreUsuario2 { get; set; }
        public string ConnectionId1 { get; set; }
        public string ConnectionId2 { get; set; }
        public char Turno { get; set; }
    }
}
