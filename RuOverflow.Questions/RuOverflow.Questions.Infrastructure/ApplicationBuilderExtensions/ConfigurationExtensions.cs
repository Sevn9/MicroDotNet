﻿using Microsoft.Extensions.Configuration;

namespace RuOverflow.Questions.Infrastructure.ApplicationBuilderExtensions;

public static class ConfigurationExtensions
{
    public static T GetSettings<T>(this IConfiguration configuration, string key) where T : new()
    {
        var settings = new T();
        configuration.GetSection(key).Bind(settings);
        return settings;
    }
}
