﻿using Microsoft.Extensions.Logging;

namespace AIOChatbot.Evals;

public class GroundTruthIngestion
{
    private readonly IEnumerable<IGroundTruthReader> groundTruthDataSources;
    private readonly ILogger<GroundTruthIngestion> logger;

    public GroundTruthIngestion(IEnumerable<IGroundTruthReader> groundTruthDataSources, ILogger<GroundTruthIngestion> logger)
    {
        this.groundTruthDataSources = groundTruthDataSources;
        this.logger = logger;
    }

    public async Task<IEnumerable<GroundTruthGroup>> RunAsync(EvaluationConfig config, CancellationToken cancellationToken)
    {
        if (config.GroundTruthsMapping is null)
        {
            logger.LogWarning("Ground truth mapping is null");
            return [];
        }

        List<GroundTruthGroup> groundTruths = [];

        foreach (var mapping in config.GroundTruthsMapping)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            var dataSource = groundTruthDataSources.FirstOrDefault(x => x.Name == mapping.Reader);
            if (dataSource is null)
            {
                logger.LogWarning("Ground truth data source {provider} not found", mapping.Reader);
                continue;
            }
            var ds = await dataSource.ReadAsync(mapping);
            logger.LogInformation("Read {count} ground truths from {provider}", ds.Count(), mapping.Reader);
            groundTruths.AddRange(ds);
        }

        return groundTruths;
    }
}
