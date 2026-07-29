const fs = require('fs');
let content = fs.readFileSync('PosCore/Views/MainWindow.xaml.cs', 'utf8');

content = content.replace(
    '        public MainWindow(MainViewModel viewModel)\n        {\n            InitializeComponent();\n            DataContext = viewModel;\n        }\n    }\n}',
    `        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void DismissHardwareError_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is ViewModels.MainViewModel vm)
            {
                vm.IsHardwareError = false;
            }
        }
    }
}`
);

fs.writeFileSync('PosCore/Views/MainWindow.xaml.cs', content);
