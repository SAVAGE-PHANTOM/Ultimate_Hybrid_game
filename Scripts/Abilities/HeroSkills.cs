using System;

public class HeroSkills {
    // Skill Attributes
    public string ActiveSkillName = "Tactical Scan";
    public float SkillCooldown = 45.0f; // Seconds
    public bool IsSkillReady = true;

    public void UseActiveSkill() {
        if (IsSkillReady) {
            IsSkillReady = false;
            Console.WriteLine("Using Skill: " + ActiveSkillName);
            // Logic for wall-hack or speed boost goes here
        }
    }
}