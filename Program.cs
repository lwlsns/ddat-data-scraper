Dictionary<string, string> professions = GetAllProfessions();
GetAllProfessionData(professions);

static Dictionary<string, string> GetAllProfessions()
{
    Dictionary<string, string> result = new Dictionary<string, string>();
    HttpClient client = new HttpClient();
    string page = client.GetStringAsync(@"https://www.gov.uk/government/collections/digital-data-and-technology-profession-capability-framework").Result;
    StringReader strReader = new StringReader(page);
    while(true)
    {
        string aLine = strReader.ReadLine();
        if(aLine != null)
        {
            if(aLine.Contains("gem-c-document-list__item-title"))
            {
                int start = aLine.IndexOf("href=\"")+"href=\"".Length;
                int end = aLine.IndexOf("\">");
                string url = aLine.Substring(start, end-start);

                start = aLine.IndexOf("\">")+"\">".Length;
                end = aLine.IndexOf("</a>");
                string title = aLine.Substring(start, end-start)
                    .ToLower()
                    .Replace(" ", "-")
                    .Replace("(","")
                    .Replace(")","");

                result.Add(title, url);
            }
        }
        else
        {
            break;
        }
    }
    
    return result;
}

static void GetAllProfessionData(Dictionary<string, string> professions)
{
    foreach(KeyValuePair<string, string> kvp in professions)
    {
        ScrapeProfession(kvp.Key, kvp.Value);
    }

}

static void ScrapeProfession(string professionTitle, string link)
{
    HttpClient client = new HttpClient();
    Dictionary<string, int> skills = new Dictionary<string, int>();

    Directory.CreateDirectory(professionTitle);

    string page = client.GetStringAsync("https://www.gov.uk/" + link).Result;
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
                    string docPath = Directory.GetCurrentDirectory()+"/"+professionTitle;

                    
                    using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, roleTitle.Replace(" ","-").Replace("---","-").ToLower() + ".yaml")))
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