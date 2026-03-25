﻿// Hàm này nhận ID của canvas, nhãn và dữ liệu để vẽ biểu đồ cột
window.renderBarChart = (canvasId, labels, data) => {
    const ctx = document.getElementById(canvasId);
    if (!ctx) {
        console.error(`Canvas element with id '${canvasId}' not found.`);
        return;
    }

    // Hủy biểu đồ cũ nếu đã tồn tại trên canvas này để tránh vẽ chồng chéo
    const existingChart = Chart.getChart(ctx);
    if (existingChart) {
        existingChart.destroy();
    }

    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: 'Doanh thu',
                data: data,
                backgroundColor: 'rgba(139, 111, 90, 0.6)', // Màu nền của cột
                borderColor: 'rgba(139, 111, 90, 1)',     // Màu viền của cột
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        // Định dạng lại nhãn trục Y cho dễ đọc (ví dụ: 1,000,000)
                        callback: function (value) {
                            return value.toLocaleString('vi-VN');
                        }
                    }
                }
            }
        }
    });
};