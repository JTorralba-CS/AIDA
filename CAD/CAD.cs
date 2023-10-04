using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

using Standard;

namespace CAD
{
    public class CAD
    {
        public static Message Response = new Message();
        public Message Command(String _Content)
        {
            Response.User = "AIDA";
            Response.Content = "";

            String[] _Arguments = _Content.Split(new String[] {" "}, StringSplitOptions.RemoveEmptyEntries);

            if (_Arguments.Length != 0)
            {
                var Command = _Arguments[0];

                if (CommandMap.ContainsKey(Command))
                {
                    CommandMap[Command](_Arguments.Skip(1).ToArray());
                }
            }

            Response.Time = DateTime.Now;

            return Response;
        }

        private static readonly Dictionary<String, Action<String[]>> CommandMap = new Dictionary<String, Action<String[]>>(StringComparer.InvariantCultureIgnoreCase)
        {
            [nameof(Push)] = Push,
            [nameof(Commit)] = Commit,
            [nameof(Login)] = Login,
            [nameof(Logout)] = Logout,
            [nameof(ShowUsers)] = ShowUsers,
            [nameof(ShowIncidents)] = ShowIncidents,
            [nameof(ShowUnits)] = ShowUnits,
            [nameof(ChangeStatus)] = ChangeStatus,
            [nameof(CrossStaff)] = CrossStaff
            //[nameof()] =
        };

        static void Push(String[] _Arguments)
        {
            Response.Content = "execute: push";
        }

        static void Commit(String[] _Arguments)
        {
            if (_Arguments.Length == 2 && _Arguments[0] == "-m")
            {
                Response.Content = $"execute: commit with message... {_Arguments[1]}";
            }
            else
            {
                Response.Content = "syntax: commit -m <some message>";
            }
        }

        static void Login(String[] _Arguments)
        {
            if (_Arguments.Length == 2)
            {
                Response.Content = $"execute: login {_Arguments[0]} {_Arguments[1]}";
            }
            else
            {
                Response.Content = "syntax: login <user> <password>";
            }
        }

        static void Logout(String[] _Arguments)
        {
            if (_Arguments.Length == 0)
            {
                Response.Content = $"execute: logout";
            }
            else
            {
                Response.Content = "syntax: logout";
            }
        }

        static void ShowUsers(String[] _Arguments)
        {
            if (_Arguments.Length == 1)
            {
                Response.Content = $"execute: showusers {_Arguments[0]}";
            }
            else
            {
                Response.Content = "syntax: showusers <filter>";
            }
        }

        static void ShowIncidents(String[] _Arguments)
        {
            if (_Arguments.Length == 1)
            {
                Response.Content = $"execute: showincidents {_Arguments[0]}";
            }
            else
            {
                Response.Content = "syntax: showincidents <filter>";
            }
        }

        static void ShowUnits(String[] _Arguments)
        {
            if (_Arguments.Length == 1)
            {
                Response.Content = $"execute: showunits {_Arguments[0]}";
            }
            else
            {
                Response.Content = "syntax: showunits <filter>";
            }
        }

        static void ChangeStatus(String[] _Arguments)
        {
            if (_Arguments.Length == 2)
            {
                Response.Content = $"execute: changestatus {_Arguments[0]} {_Arguments[1]}";
            }
            else
            {
                Response.Content = "syntax: changestatus <unit> <status>";
            }
        }

        static void CrossStaff(String[] _Arguments)
        {
            if (_Arguments.Length == 2)
            {
                Response.Content = $"execute: crossstaff {_Arguments[0]} {_Arguments[1]}";
            }
            else
            {
                Response.Content = "syntax: crossstaff <unit2,unit3,unit4,etc...> <unit1>";
            }
        }
    }
}
