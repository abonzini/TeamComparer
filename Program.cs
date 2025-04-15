using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;

namespace TeamComparer
{
    public class TeamData
    {
        public string Name;
        public bool Relevant = false;
        public HashSet<string> Mons = new HashSet<string>();
        public SortedDictionary<int, List<TeamData>> TeamBySimilarities = new SortedDictionary<int, List<TeamData>>();
    }

    internal class Program
    {
        public static int CompareTeams(TeamData t1, TeamData t2)
        {
            int res = 0;
            TeamData teamWithMostMons = (t1.Mons.Count >= t2.Mons.Count) ? t1 : t2;
            TeamData teamWithLeastMons = (t1.Mons.Count >= t2.Mons.Count) ? t2 : t1;
            foreach(string mon in teamWithMostMons.Mons)
            {
                if(teamWithLeastMons.Mons.Contains(mon))
                {
                    res++;
                }
            }
            return res;
        }
        static void Main(string[] args)
        {
            int targetTeams; // Season 3 will be checked
            List<TeamData> teams = new List<TeamData>();
            Console.WriteLine("Which season you want to check?");
            targetTeams = int.Parse(Console.ReadLine());

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
                            int similarities = CompareTeams(teams[i], teams[j]); // Compare and add to relevant teams
                            if (teams[i].Relevant)
                            {
                                if (!teams[i].TeamBySimilarities.TryGetValue(similarities, out List<TeamData> value))
                                {
                                    value = new List<TeamData>();
                                    teams[i].TeamBySimilarities[similarities] = value;
                                }
                                teams[i].TeamBySimilarities[similarities].Add(teams[j]);
                            }
                            if (teams[j].Relevant)
                            {
                                if (!teams[j].TeamBySimilarities.TryGetValue(similarities, out List<TeamData> value))
                                {
                                    value = new List<TeamData>();
                                    teams[j].TeamBySimilarities[similarities] = value;
                                }
                                teams[j].TeamBySimilarities[similarities].Add(teams[i]);
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
                        foreach(KeyValuePair<int, List<TeamData>> kvp  in team.TeamBySimilarities)
                        {
                            Console.WriteLine(kvp.Key);
                            writer.WriteLine(kvp.Key);
                            foreach (TeamData otherTeam in kvp.Value)
                            {
                                Console.WriteLine("\t" + otherTeam.Name);
                                writer.WriteLine("\t" + otherTeam.Name);
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
                        foreach (KeyValuePair<int, List<TeamData>> kvp in team.TeamBySimilarities)
                        {
                            Console.WriteLine(kvp.Key);
                            writer.WriteLine(kvp.Key);
                            foreach (TeamData otherTeam in kvp.Value)
                            {
                                if(otherTeam.Relevant)
                                {
                                    Console.WriteLine("\t" + otherTeam.Name);
                                    writer.WriteLine("\t" + otherTeam.Name);
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
