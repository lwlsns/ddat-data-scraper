
ScrapeProfession();

static void ScrapeProfession()
{
    HttpClient client = new HttpClient();
    Dictionary<string, int> skills = new Dictionary<string, int>();
    
    string page = client.GetStringAsync(@"https://www.gov.uk/guidance/development-operations-devops-engineer").Result;
    StringReader strReader = new StringReader(page);
    bool allSkillsAre1 = true;
    string roleTitle = ""; 
    while(true)
    {
        string aLine = strReader.ReadLine();
        if(aLine != null)
        {
            if(aLine.Contains("<h2"))
            {
                if(!allSkillsAre1)
                {
                    string docPath = Directory.GetCurrentDirectory();

                    
                    using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, roleTitle.Replace(" ","-").Replace("---","-").ToLower())))
                    {                     
                        outputFile.WriteLine("submitter: " + roleTitle);
                        outputFile.WriteLine("themes: # rated 1-5");
                        foreach(KeyValuePair<string, int> kvp in skills)
                        {
                            outputFile.WriteLine("  " + kvp.Key + ": " + kvp.Value);
                            skills[kvp.Key] = 1;
                        }
                    }

                }
                roleTitle = aLine.Substring(aLine.IndexOf("\">")+2).Replace("</h2>","");
                allSkillsAre1 = true;
            }
            else if(aLine.Contains("<strong>"))
            {
                int start = aLine.IndexOf("<strong>")+"<strong>".Length;
                int end = aLine.IndexOf("</strong>");
                string skill = aLine.Substring(start, end-start)
                    .ToLower()
                    .Replace(" ", "_")
                    .Replace("(","")
                    .Replace(")","");

                if(!skills.ContainsKey(skill))
                {
                    skills.Add(skill, 1);
                }
                if(aLine.Contains("Skill level:"))
                {
                    start = aLine.IndexOf("Skill level: ")+"Skill level: ".Length;
                    end = aLine.IndexOf(")</li>");
                    try
                    {
                        string skillLevel = aLine.Substring(start, end-start).ToLower();
                        switch(skillLevel)
                        {
                            case "awareness":
                                skills[skill] = 2;
                                allSkillsAre1 = false;
                                break;
                            case "working":
                                skills[skill] = 3;
                                allSkillsAre1 = false;
                                break;
                            case "practitioner":
                                skills[skill] = 4;
                                allSkillsAre1 = false;
                                break;
                            case "expert":
                                skills[skill] = 5;
                                allSkillsAre1 = false;
                                break;
                        }
                        
                    }
                    catch(Exception e)
                    {}                    
                }
            }
        }
        else
        {
            break;
        }
    }
    
}