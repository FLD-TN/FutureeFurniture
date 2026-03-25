await JS.InvokeVoidAsync(
    "chartHelper.initChart",
    "revenueBarChart",
    new
    {
        type = "bar",
        data = new {
            labels = Labels,
            datasets = new[] {
                new {
                    label = "Doanh thu (₫)",
                    data = Data,
                    backgroundColor = "rgba(54, 162, 235, 0.6)"
                }
            }
        },
        options = new { responsive = true, maintainAspectRatio = false }
    }
);
