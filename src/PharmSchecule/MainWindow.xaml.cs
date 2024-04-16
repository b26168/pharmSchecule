using System.Windows;

namespace PharmSchecule
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<string> _employees = new List<string>();
        private DayInfo[][] _days = new DayInfo[6][];
        private List<int> _offDays = new List<int>();

        public MainWindow()
        {
            InitializeComponent();

            txb_year.Text = Settings1.Default.DefaultYear;
            txb_A.Text = Settings1.Default.DefaultA;
            txb_B.Text = Settings1.Default.DefaultB;
        }

        private void Reset()
        {
            _employees = new List<string>();
            _days = new DayInfo[6][];
             _offDays = new List<int>();

            // 初始化二維陣列
            for (int i = 0; i < _days.Length; i++)
            {
                _days[i] = new DayInfo[7];
                for (int j = 0; j < _days[i].Length; j++)
                {
                    _days[i][j] = null;
                }
            }
        }

        private (int, int) StatisticsShift()
        {
            int sumA = 0;
            int sumB = 0;

            foreach (var week in _days)
            {
                SumWeek(ref sumA, ref sumB, week);
            }
            return (sumA, sumB);
        }

        private void SumWeek(ref int sumA, ref int sumB, DayInfo[] week)
        {
            foreach (var dayInfo in week)
            {
                if (dayInfo == null)
                {
                    continue;
                }

                sumA += CountEmployeeShifts(dayInfo, _employees[0]);
                sumB += CountEmployeeShifts(dayInfo, _employees[1]);
            }
        }

        private int CountEmployeeShifts(DayInfo dayInfo, string employee)
        {
            int count = 0;
            if (dayInfo.MorningShift == employee)
            {
                count++;
            }
            if (dayInfo.AfternoonShift == employee)
            {
                count++;
            }
            if (dayInfo.NightShift == employee)
            {
                count++;
            }
            return count;
        }

        private (string, string) GetRandom()
        {
            // 創建 Random 對象
            Random random = new Random();

            // 生成一個隨機索引
            int randomIndex = random.Next(0, _employees.Count);

            var firstEmployee = _employees[randomIndex];
            var secondEmployee = _employees.Where(e => e != firstEmployee).First();
            return (firstEmployee, secondEmployee);
        }

        private string GetOtherEmployee(string cuurent)
        {
            return _employees.Where(e => e != cuurent).First();
        }

        private void StartScheduling()
        {
            for (int week = 0; week < _days.Length; week++)
            {
                for (int idx = 0; idx < _days[week].Length; idx++)
                {
                    var dayInfo = _days[week][idx];
                    if (dayInfo == null) 
                    {
                        continue;
                    }
                    if (dayInfo.Off)
                    {
                        continue;
                    }

                    if (idx == 0) // 周一
                    {
                        var sundayInfo = _days[week-1][6];
                        if (sundayInfo == null || string.IsNullOrWhiteSpace(sundayInfo.MorningShift))
                        {
                            var (a, b) = GetRandom();
                            dayInfo.MorningShift = a;
                            dayInfo.AfternoonShift = a;
                            dayInfo.NightShift = b;
                        }
                        else
                        {
                            dayInfo.MorningShift = GetOtherEmployee(sundayInfo.MorningShift);
                            dayInfo.AfternoonShift = GetOtherEmployee(sundayInfo.MorningShift);
                            dayInfo.NightShift = sundayInfo.MorningShift;
                        }
                    }
                    else if (idx == 1) // 週二
                    {
                        var mondayInfo = _days[week][0];
                        if (mondayInfo == null || string.IsNullOrWhiteSpace(mondayInfo.NightShift))
                        {
                            var (a, b) = GetRandom();
                            dayInfo.MorningShift = a;
                            dayInfo.AfternoonShift = a;
                            (a, b) = GetRandom();
                            dayInfo.NightShift = a;
                        }
                        else
                        {
                            dayInfo.MorningShift = mondayInfo.NightShift;
                            dayInfo.AfternoonShift = mondayInfo.NightShift;
                            dayInfo.NightShift = GetOtherEmployee(mondayInfo.NightShift);
                        }
                    }
                    else if (idx == 3) // 週四
                    {
                        var (a, b) = GetRandom();
                        dayInfo.MorningShift = a;
                        dayInfo.AfternoonShift = a;
                        dayInfo.NightShift = a;
                    }
                    else if (idx == 4) // 週五
                    {
                        var sumA = 0;
                        var sumB = 0;
                        SumWeek(ref sumA, ref sumB, _days[week]);
                        if (sumA - sumB > 2)
                        {
                            dayInfo.MorningShift = _employees[1];
                            dayInfo.AfternoonShift = _employees[1];
                            var (a, b) = GetRandom();
                            dayInfo.NightShift = a;
                        }
                        else if (sumB - sumA > 2) 
                        {
                            dayInfo.MorningShift = _employees[0];
                            dayInfo.AfternoonShift = _employees[0];
                            var (a, b) = GetRandom();
                            dayInfo.NightShift = a;
                        }
                        else
                        {
                            AllRandom(dayInfo);
                        }
                    }    
                    else if (idx == 5) // 週六
                    {
                        var sundayInfo = _days[week][6];
                        if (sundayInfo == null || string.IsNullOrWhiteSpace(sundayInfo.MorningShift))
                        {
                            var sumA = 0;
                            var sumB = 0;
                            SumWeek(ref sumA, ref sumB, _days[week]);
                            if (sumA - sumB > 2)
                            {
                                dayInfo.MorningShift = _employees[1];
                                dayInfo.AfternoonShift = _employees[1];
                            }
                            else if (sumB - sumA > 2)
                            {
                                dayInfo.MorningShift = _employees[0];
                                dayInfo.AfternoonShift = _employees[0];
                            }
                            else
                            {
                                var (a, b) = GetRandom();
                                dayInfo.MorningShift = a;
                                dayInfo.AfternoonShift = a;
                            }
                        }
                        else
                        {
                            dayInfo.MorningShift = sundayInfo.MorningShift;
                            dayInfo.AfternoonShift = sundayInfo.MorningShift;
                        }
                    }
                    else if (idx == 6) // 週日
                    {
                        var saturdayInfo = _days[week][5];
                        if (saturdayInfo == null || string.IsNullOrWhiteSpace(saturdayInfo.MorningShift))
                        {
                            var (a, b) = GetRandom();
                            dayInfo.MorningShift = a;
                        }
                        else
                        {
                            dayInfo.MorningShift = saturdayInfo.MorningShift;
                        }
                    }
                    else
                    {
                        AllRandom(dayInfo);
                    }
                }
            }
        }

        private void AllRandom(DayInfo dayInfo)
        {
            var (a, b) = GetRandom();
            dayInfo.MorningShift = a;
            (a, b) = GetRandom();
            dayInfo.AfternoonShift = a;
            if (dayInfo.MorningShift != dayInfo.AfternoonShift)
            {
                dayInfo.NightShift = dayInfo.AfternoonShift;
            }
            else
            {
                (a, b) = GetRandom();
                dayInfo.NightShift = a;
            }
        }

        private void SetDays(int year, int month)
        {
            // 創建指定月份的第一天
            DateTime nextMonthFirstDay = new DateTime(year, month, 1);

            var week = 1;
            // 遍歷下個月的每一天，並打印每天對應的星期幾
            for (int i = 0; i < DateTime.DaysInMonth(nextMonthFirstDay.Year, nextMonthFirstDay.Month); i++)
            {
                DateTime nextDay = nextMonthFirstDay.AddDays(i);
                var dayOfWeek = nextDay.DayOfWeek;
                if (dayOfWeek == DayOfWeek.Monday && i == 0)
                // 保留第一周, 預防判斷週日時, week-1 < 0, 也可方便保留上個月的周日資訊
                {
                    week++;
                }
                var newDay = new DayInfo() { Day = i+1, DayOfWeek = dayOfWeek, Week = week, Off = _offDays.Contains(i+1) };
                var day = dayOfWeek == DayOfWeek.Sunday ? 7 : (int)dayOfWeek;
                _days[week-1][day-1] = newDay;
                
                if (dayOfWeek == DayOfWeek.Sunday)
                {
                    week++;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txb_year.Text) || string.IsNullOrWhiteSpace(txb_month.Text) )
            {
                MessageBox.Show("請輸入年月");
                return;
            }

            Reset();

            _employees.Add(txb_A.Text);
            _employees.Add(txb_B.Text);

            try
            {
                if (string.IsNullOrWhiteSpace(txb_off.Text) == false)
                {
                    _offDays = txb_off.Text.Split(',').Select(c => Convert.ToInt32(c)).ToList();
                }
            }
            catch 
            {
                MessageBox.Show("休假日 格式錯誤");
                return;
            }

            try
            {
                SetDays(Convert.ToInt32(txb_year.Text), Convert.ToInt32(txb_month.Text));
            }
            catch
            {
                MessageBox.Show("年 或 月 格式錯誤");
                return;
            }

            try
            {
                SetEmployeeOff(_employees[0], txb_Aoff.Text);
                SetEmployeeOff(_employees[1], txb_Boff.Text);
            }
            catch
            {
                MessageBox.Show("人員休假日A 或 B 格式錯誤");
                return;
            }

            StartScheduling();
            var (sumA, sumB) = StatisticsShift();

            var result = string.Empty;
            result += $"{_employees[0]}: {sumA}診, {_employees[1]}: {sumB}診\n";

            result += $",星期一,星期二,星期三,星期四,星期五,星期六,星期日\n";
            foreach (var week in _days)
            {
                var days = ",";
                var moning = "早,";
                var afternoon = "中,";
                var night = "晚,";

                var nullCount = 0;

                for (var idx = 0; idx < week.Length; idx++)
                {
                    var dayInfo = week[idx];
                    if (dayInfo == null)
                    {
                        days += ",";
                        moning += ",";
                        afternoon += ",";
                        night += ",";
                        nullCount++;
                    }
                    else if (dayInfo.Off)
                    {
                        days += $"{dayInfo.Day}";
                        moning += "休診";
                        afternoon += "休診";
                        night += "休診";
                        if (dayInfo.DayOfWeek != DayOfWeek.Sunday)
                        {
                            days += ",";
                            moning += ",";
                            afternoon += ",";
                            night += ",";
                        }
                    }
                    else if (dayInfo.DayOfWeek == DayOfWeek.Saturday)
                    {
                        days += $"{dayInfo.Day},";
                        moning += $"{dayInfo?.MorningShift},";
                        afternoon += $"{dayInfo?.AfternoonShift},";
                        night += "休診,";
                    }
                    else if (dayInfo.DayOfWeek == DayOfWeek.Sunday)
                    {
                        days += $"{dayInfo.Day}";
                        moning += $"{dayInfo?.MorningShift}";
                        afternoon += "休診";
                        night += "休診";
                    }
                    else
                    {
                        days += $"{dayInfo.Day},";
                        moning += $"{dayInfo?.MorningShift},";
                        afternoon += $"{dayInfo?.AfternoonShift},";
                        night += $"{dayInfo?.NightShift},";
                    }

                    if (idx == week.Length - 1)
                    {
                        days += "\n";
                        moning += "\n";
                        afternoon += "\n";
                        night += "\n";
                    }
                }
                if (nullCount == 7)
                {
                    continue;
                }
                result += days + moning + afternoon + night;
            }

            rtb.Text = result;
        }

        private void SetEmployeeOff(string name, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return;
            }
            var tokens = content.Split(",");
            foreach (var token in tokens)
            {
                var rlt = token.Split("-");
                var offDay = Convert.ToInt32(rlt[0]);
                var offShift = Convert.ToInt32(rlt[1]);
                foreach (var week in _days)
                {
                    foreach (var day in week)
                    {
                        if (day == null)
                        {
                            continue;
                        }
                        if (day.Day == offDay)
                        {
                            if (offShift == 1)
                            {
                                day.MorningShift = GetOtherEmployee(name);
                            }
                            else if (offShift == 2)
                            {
                                day.AfternoonShift = GetOtherEmployee(name);
                            }
                            else if (offShift == 3)
                            {
                                day.NightShift = GetOtherEmployee(name);
                            }
                            else
                            {
                                throw new Exception();
                            }
                            break;
                        }
                    }
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(rtb.Text);
            MessageBox.Show("複製成功");
        }
    }

    public class DayInfo
    {
        public bool Off { get; set; } = false;
        public int Week { get; set; }
        public int Day { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        private string _moningShift = string.Empty;
        public string MorningShift { 
            get
            {
                return _moningShift;
            }
            set 
            { 
                if (string.IsNullOrWhiteSpace(_moningShift) == false)
                {
                    return;
                }
                _moningShift = value;
            } 
        }
        private string _afternoonShift = string.Empty;
        public string AfternoonShift
        {
            get
            {
                return _afternoonShift;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(_afternoonShift) == false)
                {
                    return;
                }
                _afternoonShift = value;
            }
        }
        private string _nightShift = string.Empty;
        public string NightShift
        {
            get
            {
                return _nightShift;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(_nightShift) == false)
                {
                    return;
                }
                _nightShift = value;
            }
        }
    }
}