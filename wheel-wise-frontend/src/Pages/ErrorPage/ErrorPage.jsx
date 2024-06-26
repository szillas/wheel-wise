import {useRouteError} from "react-router-dom"
import "./ErrorPage.css";

export default function ErrorPage()
{
    const error = useRouteError();
    console.error(error);

    return(
        <div id="error-page-wrapper">
            <div id="error-page-content">
                <h1>Oops!</h1>
                <p>Sorry, an unexpected error has occured.</p>
                <p>
                    <i>{error.statusText || error.message}</i>
                    </p>
            </div>
        </div>
    );
};