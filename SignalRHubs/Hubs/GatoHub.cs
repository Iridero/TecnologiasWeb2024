using Microsoft.AspNetCore.SignalR;
using SignalRHubs.Models;

namespace SignalRHubs.Hubs
{
    public class GatoHub : Hub
    {
        public static Dictionary<string, string> usuarios =
            new Dictionary<string, string>();
        
        public static Dictionary<string, Partida> partidas = 
            new Dictionary<string, Partida>();
        
        public async Task IniciarSesion(string nombreUsuario)
        {
            //Verificar si el nombre de usuario está en uso
            if (usuarios.Keys.Any(x =>
                x.Equals(nombreUsuario
                , StringComparison.OrdinalIgnoreCase)
            ))
            {
                //Enviar mensaje de error
                await Clients.Caller.SendAsync("ReceiveMessage"
                    , "error", "El nombre de usuario ya está en uso");
            }
            else
            {
                usuarios[nombreUsuario] = Context.ConnectionId;
                await Clients.Caller.SendAsync("ReceiveMessage"
                    , "ok", "Sesión iniciada");
            }

        }

        public static Queue<string> colaUsuarios =
            new Queue<string>();

        public static int NumPartida = 0;



        public async Task BuscarPartida(string nombreUsuario)
        {
            if (colaUsuarios.Count == 0)
            {
                colaUsuarios.Enqueue(nombreUsuario);
            }
            else
            {
                var contrincante = colaUsuarios.Dequeue();
                string partida = $"partida{NumPartida}";
                await Groups.AddToGroupAsync(Context.ConnectionId, partida);
                await Groups.AddToGroupAsync(usuarios[contrincante], partida);
                NumPartida++;
                await Clients.Groups(partida).SendAsync
                    ("GameStarted", partida);
                await Clients.Users(Context.ConnectionId).SendAsync("Play","         ");
                var datosPartida = new Partida()
                {
                    NombrePartida=partida,
                    NombreUsuario1 = nombreUsuario,
                    ConnectionId1 = Context.ConnectionId,
                    NombreUsuario2=contrincante,
                    ConnectionId2 = usuarios[contrincante],
                    Turno='X'
                };
                partidas[partida] = datosPartida;
            }


        }
        public async Task Jugar(string partida, string nombreUsuario, string tablero)
        {
            var datosPartida = partidas[partida];
            if (GanaXO(datosPartida.Turno, tablero))
            {
                // Notificamos que ganó
                await Clients.Group(partida).SendAsync
                    ("GameOver", nombreUsuario);
            }
            else
            {
                datosPartida.Turno = 
                    (datosPartida.Turno == 'X' ? 'O' : 'X');
                var siguiente = datosPartida.Turno=='X'
                    ? datosPartida.ConnectionId1 
                    : datosPartida.ConnectionId2;

                await Clients.Users(siguiente).SendAsync("Play", tablero);
            }
        }

        int[,] lineas = new int[,]{
            {0,1,2},
            {3,4,5},
            {6,7,8},
            {0,3,6},
            {1,4,7},
            {2,5,8},
            {0,4,8},
            {2,4,6}
        };


        bool GanaXO(char XO, string tablero)
        {
            for (int i = 0; i < 8; i++)
            {
                if (tablero[lineas[i, 0]] == XO &&
                    tablero[lineas[i, 1]] == XO &&
                    tablero[lineas[i, 2]] == XO)
                    return true;
            }
            return false;
        }
    }
}
