import CardAd from '../../Components/CardAd';
import "./AdvertisementList.css";

export default function AdvertisementList({allAdData, handleClick, title}) {
    return (
            <div id="ad-list-wrapper">
                <h2>{title}</h2>
                <div id="ad-list-content">
                    {allAdData.map((ad) => (
                        <CardAd ad={ad} handleClick={handleClick}></CardAd>
                        ))}
                </div>
            </div>
    )

}