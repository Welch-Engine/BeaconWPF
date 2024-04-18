/** @type {import('tailwindcss').Config} */
module.exports = {
    content: ['./**/*.{razor,html}'],
    theme: {
        extend: {
            colors: {
                'white': '#F5F3F5',
                'white_normal': '#9A989A',
                'black': '#0A0A0A',
                'gray_3': '#0F0F10',
                'gray_2': '#121314',
                'gray_1': '#1D1F21',
                'red': '#EF4949',
                'orange': '#EFA73A',
                'green': '#21FF72',
                'primary': '#235DDA'
            },
            fontFamily: {
                ark: ['arkicons', 'sans'],
                gont: ['Gontserrat', 'sans']
            }
        },
    },
    future: {
        hoverOnlyWhenSupported: true,
    },
    plugins: [
        require('@tailwindcss/forms')
    ],
}

