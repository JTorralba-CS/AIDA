using System;
using System.Collections.Generic;
using System.Linq;

using Standard;

using TriTech.Common.Interface;
using TriTech.VisiCAD;
using TriTech.VisiCAD.DataService;
using TriTech.VisiCAD.Geography;
using TriTech.VisiCAD.Persons;
using TriTech.VisiCAD.Units;
using TriTech.VisiCAD.Units.Actions;
using TriTech.VisiCAD.Units.Actions.StatusChange;

namespace CAD
{
    public class CAD
    {
        private static Message Response = new Message();

        private static CADManager Manager = new CADManager();

        private static int Personnel_ID = 0;

        private static int Agency_ID = 0;

        public Message Initilaize()
        {
            Response.User = "AIDA";
            Response.Content = "";

            try
            {
                Manager.Initialize();
                Manager.ApplicationType = VisiCADDefinition.ApplicationType.NotAvailable;
                Wrapper.Initialize();
            }
            catch (Exception E)
            {
                Response.Content = E.Message;
            }

            Response.Time = DateTime.Now;

            return Response;
        }

        public Message Terminate()
        {
            Response.User = "AIDA";
            Response.Content = "";

            try
            {
                TriTech.Common.Terminator.Terminate();
            }
            catch (Exception E)
            {
                Response.Content = E.Message;
            }

            Response.Time = DateTime.Now;

            return Response;
        }

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
            [nameof(ShowOOSReasons)] = ShowOOSReasons,
            [nameof(ShowUnits)] = ShowUnits,
            [nameof(ChangeUnit)] = ChangeUnit,
            [nameof(ChangeUnitOOS)] = ChangeUnitOOS,
            [nameof(CrossStaff)] = CrossStaff,
            [nameof(CrossStaffBreak)] = CrossStaffBreak
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

                String _User = _Arguments[0];
                String _Password = _Arguments[1];

                try
                {
                    Personnel_ID = Manager.PersonLimitedQueryEngine.GetPersonnelIDByCode(_User);
                    Agency_ID = Manager.PersonLimitedQueryEngine.GetPersonnelAgencyID(Personnel_ID);

                    Manager.Login(Personnel_ID, _Password);

                    if (Manager.LoggedIn == true)
                    {
                        Response.Content = $"_ ID #{Personnel_ID} successfully logged into CAD.";
                    }
                    else
                    {
                        Response.Content = $"_ ID #{Personnel_ID} NOT successfully logged into CAD.";
                    };
                }
                catch (Exception E)
                {
                    Response.Content = E.Message;
                }
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
                try
                {
                    Manager.Logout();
                    if (Manager.LoggedIn == false)
                    {
                        Response.Content = $"_ ID #{Personnel_ID} successfully logged out of CAD.";
                    }
                    else
                    {
                        Response.Content = $"_ ID #{Personnel_ID} NOT successfully logged out of CAD.";
                    };
                }
                catch (Exception E)
                {
                    Response.Content = E.Message;
                }
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
                try
                {
                    List<Personnel> Users = Manager.PersonQueryEngine.GetPersonnel();

                    if (Users != null)
                    {
                        String Result;
                        
                        Result = "User Count = " + Users.Count().ToString() + Convert.ToChar(30);

                        if (_Arguments[0] == "*")
                        {
                            _Arguments[0] = "";
                        }

                        var Filter = Users
                            .Where(X =>
                                !(X.Code == null || X.Name == null) &&
                                !(X.Code.ToLower().Contains("xxx") || X.Name.ToLower().Contains("xxx")) &&
                                (X.Code.ToLower().Contains(_Arguments[0].ToLower()) || X.Name.ToLower().Contains(_Arguments[0].ToLower()))
                                )
                            .OrderBy(X => X.Code)
                            .ThenBy(X => X.Name);

                        if (Filter != null)
                        {
                            Result = "User Count = " + Filter.Count().ToString() + ":" + Convert.ToChar(30) +
                                "--------------------------------------------------" + Convert.ToChar(30);

                            foreach (var User in Filter)
                            {
                                var Result_Previous  = Result;

                                Result = Result + $"{User.Code} {User.Name}" + Convert.ToChar(30);

                                if (Result.Length > 4096)
                                {
                                    Result = Result_Previous;
                                    Result = Result + $"_ So many records. Please apply filter." + Convert.ToChar(30);
                                    break;
                                }
                            }
                            Response.Content = Result;
                        }
                    }
                    else
                    {
                        Response.Content = "_ Users is NULL.";
                    }
                }
                catch (Exception E)
                {
                    Response.Content = E.Message;
                }
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

        static void ShowOOSReasons(String[] _Arguments)
        {
            if (_Arguments.Length == 0)
            {
                try
                {
                    List<OutOfServiceReason> OutOfServiceReasons = Manager.UnitQueryEngine.GetOutOfServiceReasons();

                    if (OutOfServiceReasons != null)
                    {
                        String Result;

                        Result = "OOS Reason Count = " + OutOfServiceReasons.Count().ToString() + Convert.ToChar(30);

                        var Filter = OutOfServiceReasons
                            .Where(X =>
                                !(X.Name == null) &&
                                ((X.Name.ToLower().Contains("mech")) || (X.Name.ToLower().Contains("out of")))
                                )
                            .OrderBy(X => X.Agency)
                            .ThenBy(X => X.Name)
                            .ThenBy(X => X.ID);

                        if (Filter != null)
                        {
                            Result = "OOS Reason Count = " + Filter.Count().ToString() + Convert.ToChar(30) +
                                "--------------------------------------------------" + Convert.ToChar(30);

                            foreach (var OutOfServiceReason in Filter)
                            {
                                var Result_Previous = Result;

                                Result = Result + $"{OutOfServiceReason.Agency} {OutOfServiceReason.Name} {OutOfServiceReason.ID}" + Convert.ToChar(30);

                                if (Result.Length > 4096)
                                {
                                    Result = Result_Previous;
                                    Result = Result + $"_ So many records. Please apply filter." + Convert.ToChar(30);
                                    break;
                                }
                            }
                            Response.Content = Result;
                        }
                    }
                    else
                    {
                        Response.Content = "_ OutOfServiceReasons is NULL.";
                    }
                }
                catch (Exception E)
                {
                    Response.Content = E.Message;
                }
            }
            else
            {
                Response.Content = "syntax: showoosreasons";
            }
        }

        static void ShowUnits(String[] _Arguments)
        {

            if (_Arguments.Length == 1)
            {
                try
                {
                    List<Vehicle> Units = Manager.UnitQueryEngine.GetVehicles();

                    if (Units != null)
                    {
                        String Result;

                        Result = "Unit Count = " + Units.Count().ToString() + Convert.ToChar(30);

                        if (_Arguments[0] == "*")
                        {
                            _Arguments[0] = "";
                        }

                        var Filter = Units
                            .Where(X =>
                                !(X.Name == null) &&
                                !(X.Name.ToLower().Contains("xxx")) &&
                                (X.Name.ToLower().Contains(_Arguments[0].ToLower()))
                                )
                            .OrderBy(X => X.Name);

                        if (Filter != null)
                        {
                            Result = "Unit Count = " + Filter.Count().ToString() + ":" + Convert.ToChar(30) +
                                "--------------------------------------------------" + Convert.ToChar(30);

                            foreach (var Unit in Filter)
                            {
                                var Result_Previous = Result;

                                Result = Result + $"{Unit.Name} {Unit.Status} {Unit.Location}" + Convert.ToChar(30);

                                if (Result.Length > 4096)
                                {
                                    Result = Result_Previous;
                                    Result = Result + $"_ So many records. Please apply filter." + Convert.ToChar(30);
                                    break;
                                }
                            }
                            Response.Content = Result;
                        }
                    }
                    else
                    {
                        Response.Content = "_ Units is NULL.";
                    }
                }
                catch (Exception E)
                {
                    Response.Content = E.Message;
                }
                
            }
            else
            {
                Response.Content = "syntax: showunits <filter>";
            }
        }

        static void ChangeUnit(String[] _Arguments)
        {
            if (_Arguments.Length == 2)
            {
                try
                {
                    Vehicle Unit = Manager.UnitQueryEngine.GetVehicleByName(_Arguments[0].ToUpper());

                    if (Unit != null)
                    {
                        switch (_Arguments[1].ToUpper())
                        {
                            case "A":
                                ChangeStatusToAvailableParameters AvailableParameters = new ChangeStatusToAvailableParameters(Manager, Unit.ID);
                                AvailableParameters.ActionTime = DateTime.Now;
                                Manager.UnitActionEngine.ChangeStatusToAvailable(AvailableParameters);
                                break;
                            case "IQ":
                                ChangeStatusToInQuartersParameters InQuartersParameters = new ChangeStatusToInQuartersParameters(Manager, Unit.ID);
                                InQuartersParameters.ActionTime = DateTime.Now;
                                InQuartersParameters.StationID = Unit.CurrentStationID;
                                InQuartersParameters.ValidateParameters();
                                Manager.UnitActionEngine.ChangeStatusToInQuarters(InQuartersParameters);
                                break;
                            case "OFF":
                                ChangeStatusToOffDutyParameters OffDutyParameters = new ChangeStatusToOffDutyParameters(Manager, Unit.ID);
                                OffDutyParameters.ActionTime = DateTime.Now;
                                OffDutyParameters.ValidateParameters();
                                Manager.UnitActionEngine.ChangeStatusToOffDuty(OffDutyParameters);
                                break;
                            default:
                                break;
                        }

                        Unit = Manager.UnitQueryEngine.GetVehicleByName(_Arguments[0].ToUpper());
                        Response.Content = $"{Unit.Name} {Unit.Status} {Unit.Location}" + Convert.ToChar(30);
                    }
                    else
                    {
                        Response.Content = "_ Unit is NOT found.";
                    }
                }
                catch (Exception E)
                {
                    Response.Content = E.Message;
                }
            }
            else
            {
                Response.Content = "syntax: changestatus <unit> <status>";
            }
        }

        static void ChangeUnitOOS(String[] _Arguments)
        {

            if (_Arguments.Length == 2)
            {
                try
                {
                    Vehicle Unit = Manager.UnitQueryEngine.GetVehicleByName(_Arguments[0].ToUpper());
                    GeographicPoint UnitP = new GeographicPoint(Unit.Latitude, Unit.Longitude);
                    GeographicLocation UnitL = new GeographicLocation(UnitP);

                    if (Unit != null)
                    {
                        switch (_Arguments[1].ToUpper())
                        {
                            case "OOS":
                                AddOutOfServiceReasonToUnitParameters OutOfServiceParameters = new AddOutOfServiceReasonToUnitParameters(Manager, Unit.ID, 1052, true, Unit.CurrentDivisionID, UnitL);
                                Manager.UnitActionEngine.AddOutOfServiceReasonToUnit(OutOfServiceParameters);
                                break;
                            default:
                                break;
                        }

                        Unit = Manager.UnitQueryEngine.GetVehicleByName(_Arguments[0].ToUpper());
                        Response.Content = $"{Unit.Name} {Unit.Status} {Unit.Location}" + Convert.ToChar(30);
                    }
                    else
                    {
                        Response.Content = "_ Unit is NOT found.";
                    }
                }
                catch (Exception E)
                {
                    Response.Content = E.Message;
                }
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

                try
                {
                    String ObjectName = "clsAction";
                    String MethodName = "ActivateUnitCrossStaff";

                    List<String> UnitList = _Arguments[0].ToUpper().Split(new String[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    List<String> CrossWithUnitList = _Arguments[1].ToUpper().Split(new String[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();

                    UnitList.Add(String.Empty);
                    CrossWithUnitList.Add(String.Empty);

                    UnitList.Sort();
                    CrossWithUnitList.Sort();

                    Wrapper.ExecuteCommand(ObjectName, MethodName, UnitList.ToArray(), CrossWithUnitList.ToArray());
                }
                catch (Exception E)
                {
                    Response.Content = E.Message;
                }
            }
            else
            {
                Response.Content = "syntax: crossstaff <unit2,unit3,unit4,etc...> <unit1>";
            }
        }

        static void CrossStaffBreak(String[] _Arguments)
        {
            if (_Arguments.Length == 1)
            {
                try
                {
                    String ObjectName = "clsAction";
                    String MethodName = "ActivateUnitBreakCrossStaffing";

                    List<String> UnitList = _Arguments[0].ToUpper().Split(new String[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    List<String> EmptyList = String.Empty.ToUpper().Split(new String[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();

                    UnitList.Add(String.Empty);
                    EmptyList.Add(String.Empty);

                    UnitList.Sort();
                    EmptyList.Sort();

                    Wrapper.ExecuteCommand(ObjectName, MethodName, UnitList.ToArray(), EmptyList.ToArray());
                }
                catch (Exception E)
                {
                    Response.Content = E.Message;
                }
            }
            else
            {
                Response.Content = "syntax: crossstaffbreak <unit1,unit2,unit3,unit4,etc...>";
            }
        }
    }
}
