/** @type {import('tailwindcss').Config} */
module.exports = {
    content: ["../**/*{html,razor,js,cs}"],
    theme: {
        screens: {
            'sm': '640px',
            'md': '768px',
            'dl': '910px',
            'lg': '1024px',
            'xl': '1280px',
            '2xl': '1536px'
        },
        extend: {
            colors: {

                'yell':'#F7D44C',
                'salmon':'#EB7A53',
                'glass':'#98B7DB',
                'cream-green':'#A8D672',

                'current': {
                    '50': '#fef9e8',
                    '100': '#fef0c3',
                    '200': '#fee28a',
                    '300': '#fdd147',
                    '400': '#fac215',
                    '500': '#eab308',
                    '600': '#ca9a04',
                    '700': '#a17c07',
                    '800': '#85680e',
                    '900': '#715a12',
                    '950': '#423306'
                }
            }
        },
    },
    plugins: [],
}

