﻿namespace VGManager.Services.Models;

public class MatchedSecret
{
    public string SecretName { get; set; } = null!;
    public string SecretValue { get; set; } = null!;
    public string CreatedBy { get; set; } = null!;
}
