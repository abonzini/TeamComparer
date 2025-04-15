using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;

namespace TeamComparer
{
    public class TeamMatch
    {
        public TeamData Team;
        public List<string> MonsInCommon;
    }

    public class TeamData
    {
        public string Name;
        public bool Relevant = false;
        public HashSet<string> Mons = new HashSet<string>();
        public SortedDictionary<int, List<TeamMatch>> TeamBySimilarities = new SortedDictionary<int, List<TeamMatch>>();
    }

    internal class Program
    {
        public static Tuple<int, List<string>> CompareTeams(TeamData t1, TeamData t2)
        {
            int res = 0;
            List<string> monsInCommon = new List<string>();
            TeamData teamWithMostMons = (t1.Mons.Count >= t2.Mons.Count) ? t1 : t2;
            TeamData teamWithLeastMons = (t1.Mons.Count >= t2.Mons.Count) ? t2 : t1;
            foreach(string mon in teamWithMostMons.Mons)
            {
                if(teamWithLeastMons.Mons.Contains(mon))
                {
                    res++;
                    monsInCommon.Add(mon);
                }
            }
            return new Tuple<int, List<string>>(res, monsInCommon);
        }
        static void Main(string[] args)
        {
            int targetTeams; // Season 3 will be checked
            int minScore;
            List<TeamData> teams = new List<TeamData>();
            Console.WriteLine("Which season you want to check?");
            targetTeams = int.Parse(Console.ReadLine());
            Console.WriteLine("Min score to show?");
            minScore = int.Parse(Console.ReadLine());

            string[] lines = File.ReadAllLines(".\\Teams.csv");

            using (StreamWriter writer = new StreamWriter(".\\output.txt"))
            {
                // Load team data
                foreach (string line in lines)
                {
                    TeamData team = new TeamData();
                    string[] entries = line.Split(',');
                    team.Name = entries[0].Trim();
                    if((team.Name.Last() - '0') == targetTeams)
                    {
                        team.Relevant = true;
                    }
                    for(int i = 1; i < entries.Length; i++)
                    {
                        if (entries[i].Trim() != "")
                        {
                            team.Mons.Add(entries[i].ToLower().Trim());
                        }
                    }
                    teams.Add(team);
                }

                // Now I iterate, O(N^2 half triangle)
                for (int i = 0; i < teams.Count; i++) // All teams will be compared
                {
                    for (int j = i+1; j < teams.Count; j++) // No need to compare with myself, and order doesn't matter for comp
                    {
                        if (teams[i].Relevant ||  teams[j].Relevant)
                        {
                            Tuple<int, List<string>> similarities = CompareTeams(teams[i], teams[j]); // Compare and add to relevant teams
                            if (teams[i].Relevant)
                            {
                                if (!teams[i].TeamBySimilarities.TryGetValue(similarities.Item1, out List<TeamMatch> value))
                                {
                                    value = new List<TeamMatch>();
                                    teams[i].TeamBySimilarities[similarities.Item1] = value;
                                }
                                TeamMatch newMatch = new TeamMatch() { MonsInCommon = similarities.Item2, Team = teams[j] };
                                teams[i].TeamBySimilarities[similarities.Item1].Add(newMatch);
                            }
                            if (teams[j].Relevant)
                            {
                                if (!teams[j].TeamBySimilarities.TryGetValue(similarities.Item1, out List<TeamMatch> value))
                                {
                                    value = new List<TeamMatch>();
                                    teams[j].TeamBySimilarities[similarities.Item1] = value;
                                }
                                TeamMatch newMatch = new TeamMatch() { MonsInCommon = similarities.Item2, Team = teams[i] };
                                teams[j].TeamBySimilarities[similarities.Item1].Add(newMatch);
                            }
                        }
                    }
                }

                // Ok now I just print I guess?
                Console.WriteLine("===== COMPARISON WITH ALL =====");
                writer.WriteLine("===== COMPARISON WITH ALL =====");
                foreach (TeamData team in teams)
                {
                    if(team.Relevant) // Print data of team
                    {
                        Console.WriteLine(team.Name);
                        writer.WriteLine(team.Name);
                        foreach(KeyValuePair<int, List<TeamMatch>> kvp  in team.TeamBySimilarities)
                        {
                            if(kvp.Key >= minScore)
                            {
                                Console.WriteLine("\t- "+kvp.Key+" mons in common:");
                                writer.WriteLine("\t- " + kvp.Key+ " mons in common:");
                                foreach (TeamMatch otherTeam in kvp.Value)
                                {
                                    Console.Write("\t\t- " + otherTeam.Team.Name + ": Shared ");
                                    writer.Write("\t\t- " + otherTeam.Team.Name + ": Shared ");
                                    Console.WriteLine(String.Join(",", otherTeam.MonsInCommon));
                                    writer.WriteLine(String.Join(",", otherTeam.MonsInCommon));
                                }
                            }
                        }
                    }
                }
                Console.WriteLine("===== COMPARISON WITH RELEVANT =====");
                writer.WriteLine("===== COMPARISON WITH RELEVANT =====");
                foreach (TeamData team in teams)
                {
                    if (team.Relevant) // Print data of team
                    {
                        Console.WriteLine(team.Name);
                        writer.WriteLine(team.Name);
                        foreach (KeyValuePair<int, List<TeamMatch>> kvp in team.TeamBySimilarities)
                        {
                            if (kvp.Key >= minScore)
                            {
                                Console.WriteLine("\t- " + kvp.Key + " mons in common:");
                                writer.WriteLine("\t- " + kvp.Key + " mons in common:");
                                foreach (TeamMatch otherTeam in kvp.Value)
                                {
                                    if (otherTeam.Team.Relevant)
                                    {
                                        Console.Write("\t\t- " + otherTeam.Team.Name + ": Shared ");
                                        writer.Write("\t\t- " + otherTeam.Team.Name + ": Shared ");
                                        Console.WriteLine(String.Join(",", otherTeam.MonsInCommon));
                                        writer.WriteLine(String.Join(",", otherTeam.MonsInCommon));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            Console.ReadLine();
        }
    }
}
