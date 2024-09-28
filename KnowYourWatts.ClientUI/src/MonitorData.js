class MonitorData
{
    // Defines data items which will be used as display items on the monitor
    constructor(totalBill, currentRateMoney, currentRateKwh)
    {
        this.totalBill = totalBill;
        this.currentRateMoney = currentRateMoney;
        this.currentRateKwh = currentRateKwh;
    }

    getData() {
        return {
            totalBill: this.totalBill,
            currentRateMoney: this.currentRateMoney,
            currentRateKwh: this.currentRateKwh
        };
    }
}

export default MonitorData;