import styled from 'styled-components';

export const AppHeader = styled.header`
    text-align: center;
    vertical-align: central;
    padding: 2px;
    background-color: #323232;
    color: #fff;
    font-size: 24px;
`;

export const TabPane = styled.div.attrs((props) => ({
    className: 'py-4 '
}))`
    background-color: #323232;
    color: #ECECEC;
    border-radius: 10px;
`;

export const Usage = styled.div.attrs((props) => ({
    className: 'col-10 align-top fs-4 fw-bold px-4'
}))``;

export const NavItem = styled.li.attrs((props) => ({
    className: 'nav-item pe-1'
}))``;

export const NavTabs = styled.ul.attrs((props) => ({
    className: 'nav nav-tabs nav-justified py-1'
}))`
border-color: transparent !important;
`;