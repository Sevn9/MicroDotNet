﻿using RuOverflow.Questions.Features.Questions.Models;
using RuOverflow.Questions.Infrastructure.Entity;

namespace RuOverflow.Questions.Features.Tags.Models;

public class Tag : ModifiableEntity
{
#nullable disable
    protected Tag()
    {
    }
#nullable enable


    public Tag(string name, string? description = null)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
    }

    public string Name { get; set; }

    public string? Description { get; set; }

    public List<Question>? Questions { get; set; }
}
